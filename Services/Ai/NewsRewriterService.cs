using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MoodNews.Data;
using MoodNews.DTOs; // Подключаем пространство имен DTO
using MoodNews.Entities;

namespace MoodNews.Services.Ai
{
    public class NewsRewriterService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        private static string? _cachedAccessToken;
        private static DateTime _tokenExpiresAt = DateTime.MinValue;

        public NewsRewriterService(AppDbContext context, HttpClient httpClient, IConfiguration config)
        {
            _context = context;
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<NewsRewriteDto> GetOrGenerateRewriteAsync(int newsId, string mood)
        {
            mood = mood.ToLower().Trim();

            // 1. Проверяем кэш в БД
            var existing = await _context.NewsRewrites
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.NewsId == newsId && r.Mood == mood);

            if (existing != null)
            {
                // Мапим сущность из БД в чистый DTO
                return MapToDto(existing);
            }

            // 2. Достаем оригинальную новость
            var news = await _context.News.FindAsync(newsId);
            if (news == null)
                throw new Exception("Новость с таким ID не найдена в базе данных.");

            if (mood == "neutral")
            {
                var neutralRewrite = new NewsRewrite
                {
                    NewsId = newsId,
                    Mood = "neutral",
                    RewrittenTitle = news.Title,
                    RewrittenText = news.OriginalText,
                    CreatedAt = DateTime.UtcNow
                };

                return MapToDto(neutralRewrite);
            }

            // 3. Вызываем GigaChat API
            var (title, text) = await CallGigaChatApiAsync(news.Title, news.OriginalText, mood);

            // 4. Сохраняем результат в БД
            var rewrite = new NewsRewrite
            {
                NewsId = newsId,
                Mood = mood,
                RewrittenTitle = title,
                RewrittenText = text,
                CreatedAt = DateTime.UtcNow
            };

            _context.NewsRewrites.Add(rewrite);
            await _context.SaveChangesAsync();

            return MapToDto(rewrite);
        }

        // Вспомогательный маппер (превращает сущность БД в объект для API)
        private static NewsRewriteDto MapToDto(NewsRewrite entity)
        {
            return new NewsRewriteDto
            {
                Id = entity.Id,
                NewsId = entity.NewsId,
                Mood = entity.Mood,
                RewrittenTitle = entity.RewrittenTitle,
                RewrittenText = entity.RewrittenText,
                CreatedAt = entity.CreatedAt
            };
        }

        private async Task<(string Title, string Text)> CallGigaChatApiAsync(string origTitle, string origText, string mood)
        {
            var authKey = _config["AiSettings:ApiKey"];
            if (string.IsNullOrWhiteSpace(authKey) || authKey.Contains("ВАШ"))
            {
                throw new Exception("Укажите валидный ключ GigaChat в appsettings.json!");
            }

            string accessToken = await GetGigaChatAccessTokenAsync(authKey);

            string moodInstructions = mood.ToLower() switch
            {
                "joyful" => "Максимально РАДОСТНЫЙ, восторженный и эйфорический тон. Используй восклицания и ярко позитивные эпитеты ('потрясающе', 'замечательно', 'ура').",
                "sad" => "ГЛУБОКО ПЕЧАЛЬНЫЙ, трагический и меланхоличный тон. Подчеркни грусть ситуации ('к сожалению', 'увы', 'печально', 'безысходность').",
                "ironic" => "ЯДОВИТАЯ ИРОНИЯ, сарказм и сатира. Используй колкости и скепсис ('ну конечно', 'кто бы сомнения имел', 'очередной шедевр').",
                "dramatic" => "ОСТРОСЮЖЕТНАЯ ДРАМА, максимальный накал страстей, шок и интрига ('катастрофа', 'невероятно', 'шок', 'на грани').",
                _ => "Нейтральный стилистический пересказ."
            };

            string systemPrompt = $@"
Ты — профессиональный эмоциональный копирайтер.
Твоя задача — ПОЛНОСТЬЮ переписать заголовок и текст новости, придав им ядовито-яркую эмоцию.

ЭМОЦИОНАЛЬНАЯ УСТАНОВКА:
{moodInstructions}

ОБЯЗАТЕЛЬНЫЕ ПРАВИЛА:
1. Перепиши текст РАДИКАЛЬНО, изменяя словарный запас под требуемую эмоцию. Нейтральный тон ЗАПРЕЩЕН.
2. СОХРАНИ основные факты (имена, даты, цифры, места).
3. НЕ ИСПОЛЬЗУЙ разметку ```json. Начинай ответ сразу с символa {{ и заканчивай }}:
{{
  ""title"": ""Новый переписанный заголовок"",
  ""text"": ""Новый переписанный текст новости""
}}
";

            string userPrompt = $"Исходный заголовок: {origTitle}\n\nИсходный текст: {origText}";

            var body = new
            {
                model = "GigaChat",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.85
            };

            string jsonResponse = string.Empty;
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://gigachat.devices.sberbank.ru/api/v1/chat/completions");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

                    var response = await _httpClient.SendAsync(request);
                    jsonResponse = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Ошибка GigaChat API ({response.StatusCode}): {jsonResponse}");
                    }
                    break;
                }
                catch (Exception) when (attempt == 1)
                {
                    await Task.Delay(350);
                }
            }

            using var doc = JsonDocument.Parse(jsonResponse);
            string rawContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";

            string cleanedContent = CleanJsonContent(rawContent);

            try
            {
                using var contentDoc = JsonDocument.Parse(cleanedContent);
                string newTitle = contentDoc.RootElement.GetProperty("title").GetString() ?? origTitle;
                string newText = contentDoc.RootElement.GetProperty("text").GetString() ?? cleanedContent;
                return (newTitle, newText);
            }
            catch
            {
                return ($"[{mood.ToUpper()}] {origTitle}", cleanedContent);
            }
        }

        private async Task<string> GetGigaChatAccessTokenAsync(string authKey)
        {
            if (!string.IsNullOrEmpty(_cachedAccessToken) && DateTime.UtcNow < _tokenExpiresAt)
            {
                return _cachedAccessToken;
            }

            for (int attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://ngw.devices.sberbank.ru:9443/api/v2/oauth");

                    string cleanKey = authKey.Trim();
                    if (cleanKey.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                    {
                        cleanKey = cleanKey.Substring(6).Trim();
                    }

                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", cleanKey);
                    request.Headers.Add("RqUID", Guid.NewGuid().ToString());

                    var contentList = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("scope", "GIGACHAT_API_PERS")
                    };
                    request.Content = new FormUrlEncodedContent(contentList);

                    var response = await _httpClient.SendAsync(request);
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Ошибка авторизации GigaChat OAuth ({response.StatusCode}): {jsonResponse}");
                    }

                    using var doc = JsonDocument.Parse(jsonResponse);
                    _cachedAccessToken = doc.RootElement.GetProperty("access_token").GetString()!;

                    long expiresAtUnix = doc.RootElement.GetProperty("expires_at").GetInt64();
                    _tokenExpiresAt = DateTimeOffset.FromUnixTimeMilliseconds(expiresAtUnix).UtcDateTime.AddMinutes(-2);

                    return _cachedAccessToken;
                }
                catch (Exception) when (attempt == 1)
                {
                    await Task.Delay(350);
                }
            }

            throw new Exception("Не удалось получить OAuth токен GigaChat после 2 попыток соединения.");
        }

        private static string CleanJsonContent(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "{}";

            string result = raw.Trim();

            if (result.StartsWith("```"))
            {
                int firstLineEnd = result.IndexOf('\n');
                if (firstLineEnd != -1)
                {
                    result = result.Substring(firstLineEnd + 1);
                }
                if (result.EndsWith("```"))
                {
                    result = result.Substring(0, result.Length - 3);
                }
            }

            return result.Trim();
        }
    }
}
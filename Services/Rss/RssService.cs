using CodeHollow.FeedReader;
using Microsoft.EntityFrameworkCore;
using MoodNews.Data;
using MoodNews.Entities;

namespace MoodNews.Services.Rss
{
    public class RssService : IRssService
    {
        private readonly AppDbContext _context;

        public RssService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> FetchAndSaveNewsAsync(string rssUrl)
        {
            // 1. Считываем RSS-ленту с помощью CodeHollow.FeedReader
            var feed = await FeedReader.ReadAsync(rssUrl);

            int addedCount = 0;

            foreach (var item in feed.Items)
            {
                // Пропускаем записи без ссылки или заголовка
                if (string.IsNullOrWhiteSpace(item.Link) || string.IsNullOrWhiteSpace(item.Title))
                    continue;

                // 2. Проверяем, нет ли уже этой новости в базе по SourceUrl (чтобы не спамить дублями)
                bool exists = await _context.News.AnyAsync(n => n.SourceUrl == item.Link);
                if (exists)
                    continue;

                // Извлекаем текст новости (берем Description, а если его нет — Content)
                string rawText = item.Description ?? item.Content ?? item.Title;

                // Очищаем HTML-теги, если RSS возвращает текст с разметкой
                string cleanText = System.Text.RegularExpressions.Regex.Replace(rawText, "<.*?>", string.Empty).Trim();

                var news = new News
                {
                    Title = item.Title.Trim(),
                    OriginalText = cleanText,
                    SourceUrl = item.Link.Trim(),
                    SourceName = string.IsNullOrWhiteSpace(feed.Title) ? "RSS Source" : feed.Title.Trim(),
                    PublishedAt = item.PublishingDate ?? DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.News.Add(news);
                addedCount++;
            }
            // 3. Сохраняем все добавленные новости в MySQL
            if (addedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return addedCount;
        }
    }
}

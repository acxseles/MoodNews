using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoodNews.Data;
using MoodNews.Services.Ai;
using MoodNews.Services.Rss;

namespace MoodNews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly IRssService _rssService;
        private readonly AppDbContext _context;
        private readonly NewsRewriterService _newsRewriterService; // 1. Объявляем поле

        // 2. Внедряем сервис через конструктор
        public NewsController(
            IRssService rssService,
            AppDbContext context,
            NewsRewriterService newsRewriterService)
        {
            _rssService = rssService;
            _context = context;
            _newsRewriterService = newsRewriterService;
        }

        /// <summary>
        /// Эндпоинт для запуска импорта новостей из RSS
        /// </summary>
        [HttpPost("fetch-rss")]
        public async Task<IActionResult> FetchRss([FromQuery] string? feedUrl)
        {
            string urlToFetch = string.IsNullOrWhiteSpace(feedUrl)
                ? "https://habr.com/ru/rss/news/all/?fl=ru"
                : feedUrl;

            try
            {
                int addedCount = await _rssService.FetchAndSaveNewsAsync(urlToFetch);
                return Ok(new { message = $"Импортировано новых новостей: {addedCount}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить список всех загруженных новостей
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllNews()
        {
            var newsList = await _context.News
                .OrderByDescending(n => n.PublishedAt)
                .Take(20)
                .ToListAsync();

            return Ok(newsList);
        }

        /// <summary>
        /// Получить или сгенерировать переписанную новость в заданном настроении
        /// </summary>
        [HttpGet("{id}/rewrite")]
        public async Task<IActionResult> GetRewrite(int id, [FromQuery] string mood)
        {
            try
            {
                var result = await _newsRewriterService.GetOrGenerateRewriteAsync(id, mood);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Вместо падения сервера возвращаем структурированный 500 JSON
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
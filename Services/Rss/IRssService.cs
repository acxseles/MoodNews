using MoodNews.Entities;

namespace MoodNews.Services.Rss
{
    public interface IRssService
    {
        /// <summary>
        /// Парсит RSS-ленту и сохраняет новые статьи в MySQL
        /// </summary>
        Task<int> FetchAndSaveNewsAsync(string rssUrl);
    }
}

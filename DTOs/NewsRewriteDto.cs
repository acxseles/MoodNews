namespace MoodNews.DTOs
{
    public class NewsRewriteDto
    {
        public int Id { get; set; }
        public int NewsId { get; set; }
        public string Mood { get; set; } = string.Empty;
        public string RewrittenTitle { get; set; } = string.Empty;
        public string RewrittenText { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace MoodNews.Entities;

public partial class News
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string OriginalText { get; set; } = null!;

    public string SourceUrl { get; set; } = null!;

    public string SourceName { get; set; } = null!;

    public DateTime PublishedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<NewsRewrite> NewsRewrites { get; set; } = new List<NewsRewrite>();
}

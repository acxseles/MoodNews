using System;
using System.Collections.Generic;

namespace MoodNews.Entities;

public partial class NewsRewrite
{
    public int Id { get; set; }

    public int NewsId { get; set; }

    public string Mood { get; set; } = null!;

    public string RewrittenTitle { get; set; } = null!;

    public string RewrittenText { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual News News { get; set; } = null!;
}

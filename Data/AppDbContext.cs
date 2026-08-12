using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MoodNews.Entities;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace MoodNews.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<NewsRewrite> NewsRewrites { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=mood_news_db;user=root;password=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.41-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("news")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.SourceUrl, "unique_source_url")
                .IsUnique()
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 255 });

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OriginalText)
                .HasColumnType("text")
                .HasColumnName("original_text");
            entity.Property(e => e.PublishedAt)
                .HasColumnType("datetime")
                .HasColumnName("published_at");
            entity.Property(e => e.SourceName)
                .HasMaxLength(255)
                .HasColumnName("source_name");
            entity.Property(e => e.SourceUrl)
                .HasMaxLength(1024)
                .HasColumnName("source_url");
            entity.Property(e => e.Title)
                .HasMaxLength(512)
                .HasColumnName("title");
        });

        modelBuilder.Entity<NewsRewrite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("news_rewrites")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.NewsId, e.Mood }, "unique_news_mood").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Mood)
                .HasMaxLength(50)
                .HasColumnName("mood");
            entity.Property(e => e.NewsId).HasColumnName("news_id");
            entity.Property(e => e.RewrittenText)
                .HasColumnType("text")
                .HasColumnName("rewritten_text");
            entity.Property(e => e.RewrittenTitle)
                .HasMaxLength(512)
                .HasColumnName("rewritten_title");

            entity.HasOne(d => d.News).WithMany(p => p.NewsRewrites)
                .HasForeignKey(d => d.NewsId)
                .HasConstraintName("news_rewrites_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

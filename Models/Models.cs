using System;
using System.Collections.Generic;

namespace LiranNewsSection.Functions
{
    public class NewsItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string? PolygonId { get; set; }
        public List<string> Tickers { get; set; } = new List<string>();
        public string? ArticleUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? Author { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TickerOverview
    {
        public string Ticker { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? MarketCap { get; set; }
        public string? CurrencyName { get; set; }
        public string? Market { get; set; }
        public string? Type { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 
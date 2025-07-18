using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace LiranNewsSection.Functions
{
   
    public class FetchService : IFetchService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<FetchService> _logger;
        private readonly IConfigurationService _config;

        public FetchService(ILogger<FetchService> logger, IConfigurationService config)
        {
            _logger = logger;
            _config = config;
        }

        private class NewsDbContext : DbContext
        {
            public DbSet<NewsItem> NewsItems { get; set; }
            public DbSet<TickerOverview> TickerOverviews { get; set; }
            
            private readonly string _connectionString;
            
           
            public NewsDbContext(string connectionString)
            {
                _connectionString = connectionString;
            }
            
            
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

       
      

        // Fetch news data from Polygon.io API for the current day
        public async Task<(List<NewsItem> NewsItems, int Count)> FetchNewsData()
        {
            try
            {
                // STEP 1: Construct API URL with date filter to fetch today's news
                var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
                var newsUrlWithDate = $"{_config.GetNewsUrlWithParams()}&{_config.DefaultDateFilter}={today}";
                
                _logger.LogInformation($"Fetching news data for date: {today}");

                // STEP 2: Make HTTP request to Polygon.io API and validate response
                var newsResponse = await _httpClient.GetAsync(newsUrlWithDate);
                newsResponse.EnsureSuccessStatusCode();
                var newsJson = await newsResponse.Content.ReadAsStringAsync();
                
                _logger.LogInformation($"Successfully fetched news data, response length: {newsJson.Length}");
            
                return ParseNewsResponse(newsJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching news data");
                throw;
            }
        }

        // Parse JSON response from Polygon.io API and extract news items with count information
        public (List<NewsItem> NewsItems, int Count) ParseNewsResponse(string json)
        {
            try
            {
                var list = new List<NewsItem>();
                int count = 0;
                
                using var doc = JsonDocument.Parse(json);
                
                // STEP 1: Extract the total count from the API response
                // This is used to detect if we hit the API limit (1000 items)
                if (doc.RootElement.TryGetProperty("count", out var countProp))
                {
                    count = countProp.GetInt32();
                }
                
                // STEP 2: Parse each news item from the results array
                if (doc.RootElement.TryGetProperty("results", out var results))
                {
                    foreach (var item in results.EnumerateArray())
                    {
                        try
                        {
                            list.Add(new NewsItem
                            {
                                Title = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? string.Empty : string.Empty,
                                Description = item.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? string.Empty : string.Empty,
                                Source = item.TryGetProperty("source", out var sourceProp) ? sourceProp.GetString() ?? string.Empty : string.Empty,
                                PublishedAt = item.TryGetProperty("published_utc", out var pubProp) && pubProp.ValueKind == JsonValueKind.String && DateTime.TryParse(pubProp.GetString(), out var dt) ? dt : DateTime.MinValue,
                                PolygonId = item.TryGetProperty("id", out var idProp) ? idProp.GetString() : null,
                                Tickers = item.TryGetProperty("tickers", out var tickersProp) && tickersProp.ValueKind == JsonValueKind.Array ? tickersProp.EnumerateArray().Select(t => t.GetString()).Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>() : new List<string>(),
                                ArticleUrl = item.TryGetProperty("article_url", out var urlProp) ? urlProp.GetString() : null,
                                ImageUrl = item.TryGetProperty("image_url", out var imgProp) ? imgProp.GetString() : null,
                                Author = item.TryGetProperty("author", out var authorProp) ? authorProp.GetString() : null,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse individual news item, skipping");
                            continue;
                        }
                    }
                }
                
                _logger.LogInformation($"Successfully parsed {list.Count} news items from JSON response");
                return (list, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while parsing news response");
                throw;
            }
        }

        // Fetch detailed overview information for a specific ticker from Polygon.io API
        public async Task<TickerOverview?> FetchTickerOverview(string ticker)
        {
            try
            {
                // STEP 1: Construct the API URL for the specific ticker
                var url = _config.GetTickerOverviewUrl(ticker);
                
                _logger.LogInformation($"Fetching ticker overview for: {ticker}");
                
                // STEP 2: Make HTTP request to Polygon.io ticker overview endpoint
                var resp = await _httpClient.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to fetch ticker overview for {ticker}, status code: {resp.StatusCode}");
                    return null;
                }
                    
                // STEP 3: Parse the JSON response and extract ticker overview data
                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("results", out var res))
                {
                    // STEP 4: Map API response fields to our TickerOverview model
                    var overview = new TickerOverview
                    {
                        Ticker = res.GetProperty("ticker").GetString() ?? ticker,
                        Name = res.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null,
                        Description = res.TryGetProperty("description", out var descProp) ? descProp.GetString() : null,
                        MarketCap = res.TryGetProperty("market_cap", out var mcProp) && mcProp.ValueKind == JsonValueKind.Number ? mcProp.GetDecimal() : null,
                        CurrencyName = res.TryGetProperty("currency_name", out var curProp) ? curProp.GetString() : null,
                        Market = res.TryGetProperty("market", out var mProp) ? mProp.GetString() : null,
                        Type = res.TryGetProperty("type", out var tProp) ? tProp.GetString() : null,
                        Date = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _logger.LogInformation($"Successfully fetched ticker overview for: {ticker}");
                    return overview;
                }
                
                _logger.LogWarning($"No results found in ticker overview response for: {ticker}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching ticker overview for {ticker}");
                return null;
            }
        }

        // Process news items and store them in the database with deduplication
        public async Task<(int NewNewsCount, int NewTickerCount)> ProcessNewsItems(List<NewsItem> newsItems)
        {
            try
            {
                _logger.LogInformation($"Starting to process {newsItems.Count} news items");
                
                // STEP 1: Initialize database context and identify new news items
                using var db = new NewsDbContext(_config.DatabaseConnectionString);
                var newNewsItems = new List<NewsItem>();
                var allTickers = new HashSet<string>();
                
                // STEP 2: Filter out existing news items and collect all tickers
                foreach (var news in newsItems)
                {
                    try
                    {
                        // Check if news item already exists in database
                        if (!string.IsNullOrEmpty(news.PolygonId) && !await db.NewsItems.AnyAsync(n => n.PolygonId == news.PolygonId))
                        {
                            newNewsItems.Add(news);
                            
                            // Collect all tickers from new news items
                            if (news.Tickers != null && news.Tickers.Any())
                            {
                                foreach (var ticker in news.Tickers)
                                {
                                    if (!string.IsNullOrWhiteSpace(ticker))
                                    {
                                        allTickers.Add(ticker.Trim());
                                    }
                                }
                            }
                        }
                        // Log warning for news items without ID 
                        else if (string.IsNullOrEmpty(news.PolygonId))
                        {
                            
                            _logger.LogWarning($"Skipping news item without PolygonId: {news.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to process individual news item: {news.Title}");
                        continue;
                    }
                }
                
                _logger.LogInformation($"Found {newNewsItems.Count} new news items with {allTickers.Count} unique tickers");
                
                // STEP 3: Fetch ticker overviews only for new tickers
                var newTickerCount = 0;
                foreach (var ticker in allTickers)
                {
                    try
                    {
                        // Only fetch ticker overview if it doesn't already exist in database
                        if (!await db.TickerOverviews.AnyAsync(t => t.Ticker == ticker))
                        {
                            var overview = await FetchTickerOverview(ticker);
                            if (overview != null)
                            {
                                db.TickerOverviews.Add(overview);
                                newTickerCount++;
                                _logger.LogInformation($"Stored overview for ticker: {ticker}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to process ticker overview for: {ticker}");
                        continue;
                    }
                }
                
                // STEP 4: Add and save all new news items to database
                db.NewsItems.AddRange(newNewsItems);
                var newNewsCount = newNewsItems.Count;
                await db.SaveChangesAsync();
                
                _logger.LogInformation($"Successfully processed news items. Added {newNewsCount} news items and {newTickerCount} ticker overviews");
                return (newNewsCount, newTickerCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing news items");
                throw;
            }
        }

 
       
    }
} 
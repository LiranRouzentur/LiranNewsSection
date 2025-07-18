using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.IO;
using System.Linq;

namespace LiranNewsSection.Functions
{
    public class NewsIngestionaAPITestFunction
    {
        private readonly ILogger<NewsIngestionaAPITestFunction> _logger;
        private readonly IConfigurationService _config;
        private readonly IFetchService _fetchService;

        public NewsIngestionaAPITestFunction(ILogger<NewsIngestionaAPITestFunction> logger, IConfigurationService config, IFetchService fetchService)
        {
            _logger = logger;
            _config = config;
            _fetchService = fetchService;
        }

        [Function("NewsIngestionaAPITestFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation($"News ingestion function started at: {DateTime.UtcNow}");
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            try
            {
                // STEP 1: Fetch news data from Polygon.io API
                var newsData = await _fetchService.FetchNewsData();
                
                // STEP 2: Check if we hit the API limit and log warning if so
                _logger.LogInformation($"Fetched {newsData.Count} news items for today");
                if (newsData.Count == _config.NewsLimit)
                {
                    _logger.LogWarning($"There might be data that wasn't fetched due to API limitations. Response count equals the limit of {_config.NewsLimit}.");
                }
                
                // SKIPPED - Process news items and store in database with deduplication
                // var (newNewsCount, newTickerCount) = await _newsDataService.ProcessNewsItems(newsData.NewsItems);
                
                // STEP 3: Return the exact data as JSON instead of logging
                var result = new
                {
                    NewsData = new
                    {
                        Count = newsData.Count,
                        NewsItems = newsData.NewsItems
                    },
                    ApiLimitReached = newsData.Count == _config.NewsLimit,
                    ApiLimit = _config.NewsLimit,
                    Timestamp = DateTime.UtcNow
                };
                
                var jsonResponse = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                await response.WriteStringAsync(jsonResponse);
                
                _logger.LogInformation($"News ingestion test completed successfully. Fetched {newsData.NewsItems.Count} news items.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during news ingestion test");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = "Internal server error", details = ex.Message }));
            }

            return response;
        }
    }
} 
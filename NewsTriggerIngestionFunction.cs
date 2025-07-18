using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace LiranNewsSection.Functions
{
    public class NewsTriggerIngestionFunction
    {
        private readonly ILogger<NewsTriggerIngestionFunction> _logger;
        private readonly IConfigurationService _config;
        private readonly IFetchService _fetchService;

       
        public NewsTriggerIngestionFunction(ILogger<NewsTriggerIngestionFunction> logger, IConfigurationService config, IFetchService fetchService)
        {
            _logger = logger;
            _config = config;
            _fetchService = fetchService;
        }

        // Timer-triggered function that runs every hour to fetch and process news data
        [Function("NewsTriggerIngestionFunction")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"News ingestion function started at: {DateTime.UtcNow}");
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
                
                // STEP 3: Process news items and store in database with deduplication
                var (newNewsCount, newTickerCount) = await _fetchService.ProcessNewsItems(newsData.NewsItems);
                
               
                _logger.LogInformation($"News ingestion completed successfully. Added {newNewsCount} new news items and {newTickerCount} new ticker overviews.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during news ingestion");
                throw;
            }
        }
    }
} 
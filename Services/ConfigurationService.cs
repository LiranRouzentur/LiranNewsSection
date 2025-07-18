using Microsoft.Extensions.Configuration;

namespace LiranNewsSection.Functions
{

    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;

      
        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


      
        // Polygon API Configuration, 
        // Currently stored and fetched from appsettings.json
        // In production, will be fetched from Azure keyVault
        /////////////////////////////////////////////////////////////////////
        public string PolygonApiKey => _configuration["PolygonApi:ApiKey"] ?? string.Empty;
        public string PolygonNewsUrl => _configuration["PolygonApi:NewsUrl"] ?? string.Empty;
        public string PolygonTickerOverviewUrl => _configuration["PolygonApi:TickerOverviewUrl"] ?? string.Empty;

        // Database Configuration
        public string DatabaseConnectionString => _configuration["Database:ConnectionString"] ?? string.Empty;

        // API Settings
        public int NewsLimit => int.TryParse(_configuration["ApiSettings:NewsLimit"], out int limit) ? limit : 1000;
        public string DefaultDateFilter => _configuration["ApiSettings:DefaultDateFilter"] ?? "published_utc.gte";
        
        
        
        // Build complete API URL for news endpoint with parameters
        public string GetNewsUrlWithParams()
        {
            try
            {
                if (string.IsNullOrEmpty(PolygonApiKey))
                {
                    throw new InvalidOperationException("Polygon API key is not configured");
                }
                
                if (string.IsNullOrEmpty(PolygonNewsUrl))
                {
                    throw new InvalidOperationException("Polygon news URL is not configured");
                }
                
                return $"{PolygonNewsUrl}?limit={NewsLimit}&apiKey={PolygonApiKey}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to build news URL: {ex.Message}", ex);
            }
        }

        // Build complete API URL for ticker overview endpoint with parameters
        public string GetTickerOverviewUrl(string ticker)
        {
            try
            {
                if (string.IsNullOrEmpty(ticker))
                {
                    throw new ArgumentException("Ticker symbol cannot be null or empty", nameof(ticker));
                }
                
                if (string.IsNullOrEmpty(PolygonApiKey))
                {
                    throw new InvalidOperationException("Polygon API key is not configured");
                }
                
                if (string.IsNullOrEmpty(PolygonTickerOverviewUrl))
                {
                    throw new InvalidOperationException("Polygon ticker overview URL is not configured");
                }
                
                return string.Format(PolygonTickerOverviewUrl, ticker) + $"?apiKey={PolygonApiKey}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to build ticker overview URL: {ex.Message}", ex);
            }
        }
    }
} 
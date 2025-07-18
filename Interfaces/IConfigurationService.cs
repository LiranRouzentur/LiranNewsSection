namespace LiranNewsSection.Functions
{
  
    public interface IConfigurationService
    {
        // Polygon API Configuration
        string PolygonApiKey { get; }
        string PolygonNewsUrl { get; }
        string PolygonTickerOverviewUrl { get; }

        // Database Configuration
        string DatabaseConnectionString { get; }

        // API Settings
        int NewsLimit { get; }
        string DefaultDateFilter { get; }

        // Helper methods to build complete API URLs
        string GetNewsUrlWithParams();
        string GetTickerOverviewUrl(string ticker);
    }
} 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace LiranNewsSection.Functions
{
   
    public class Program
    {
        
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables();
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(serviceCollection =>
                {
                
                    serviceCollection.AddSingleton<IConfigurationService, ConfigurationService>();
                    serviceCollection.AddSingleton<IFetchService, FetchService>();
                    serviceCollection.AddSingleton<NewsIngestionaAPITestFunction>();
                    serviceCollection.AddHttpClient();
                })
                .Build();

            host.Run();
        }
    }
} 
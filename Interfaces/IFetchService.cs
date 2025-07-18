using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiranNewsSection.Functions
{
    
    public interface IFetchService
    {
        Task<(List<NewsItem> NewsItems, int Count)> FetchNewsData();
        Task<(int NewNewsCount, int NewTickerCount)> ProcessNewsItems(List<NewsItem> newsItems);
        Task<TickerOverview?> FetchTickerOverview(string ticker);
        (List<NewsItem> NewsItems, int Count) ParseNewsResponse(string json);
    }
} 
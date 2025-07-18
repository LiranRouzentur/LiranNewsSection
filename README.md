Please refer to "News API Assignment - Liran Rouzentur.pdf" for the complete assignment answered requirements and all relevant details.

########################################################

Azure triggerd Function app that fetches and processes financial news from Polygon.io API.


1. **Logic**: 
   -  Hourly process fetches news from "https://api.polygon.io/v2/reference/news" for the full day (not last hour) for best 
      practice.
   -  Process compares by index in DB and for each new record, checks "response.tickers" array. 
      If tickers exist and not null, performs additional 
      requests to "https://api.polygon.io/v3/reference/tickers/{0}" for each ticker value, then inserts all to DB. 
   -  API has 1000 record limit without total amount info or pagination. 
      Logging system alerts when response.count equals 1000 to minimize data loss risk.

2. **Two Functions**:
   - **NewsTriggerIngestionFunction**: 
      Timer-triggered, fetches news and stores in database (No DB logic)
      `http://localhost:7074/api/NewsTriggerIngestionTestFunction`
   - **NewsTriggerIngestionTestFunction**: 
       For Testing: HTTP-triggered, same exect logic exept it fetches news and returns JSON response
      `http://localhost:7074/api/NewsIngestionaAPITestFunction`

3. **Configuration**: 
      Polygon.io API key and endpoints are configured in `appsettings.json`
      In prodoction I would use Azure Key Vault for sensitive data management.

4. **Project Structure**
   - `Interfaces/` - Service interfaces for dependency injection
   - `Services/` - Business logic service implementations
   - `Models/` - Data models
   - `NewsTriggerIngestionFunction.cs` - Timer-triggered function
   - `NewsTriggerIngestionTestFunction.cs` - HTTP test function

5. `local.settings.json` is included in the repository for easier setup.



## Prerequisites

1. **Install .NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Run installer and restart terminal

2. **Install Azure Functions Core Tools**
   - Run: `npm install -g azure-functions-core-tools@4 --unsafe-perm true`
   - Or download from: https://go.microsoft.com/fwlink/?linkid=2174087


## Setup

1. **Clone and navigate to project**
   ```bash
   git clone <repository-url>
   cd LiranNewsSection
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

## Run Locally

```bash
func start
```











 

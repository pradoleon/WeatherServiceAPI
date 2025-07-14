using MongoDB.Driver;
using WeatherService.Domain.Repositories;
using WeatherService.Domain.Entities;

namespace WeatherService.Infraestructure.Repositories
{
    /// <summary>
    /// MongoDB repository implementation for weather data
    /// </summary>
    public class WeatherRepository : IWeatherRepository
    {
        private readonly IMongoCollection<WeatherData> _weatherCollection;
        private readonly ILogger<WeatherRepository> _logger;

        public WeatherRepository(IMongoDatabase database, ILogger<WeatherRepository> logger)
        {
            _weatherCollection = database.GetCollection<WeatherData>("weather");
            _logger = logger;

            // Create indexes for better query performance
            CreateIndexes();
        }

        public async Task<WeatherData?> GetWeatherDataAsync(double latitude, double longitude)
        {
            try
            {
                var filter = Builders<WeatherData>.Filter.And(
                    Builders<WeatherData>.Filter.Gte(x => x.ExpiresAt, DateTime.UtcNow),
                    Builders<WeatherData>.Filter.Eq(x => x.Latitude, latitude),
                    Builders<WeatherData>.Filter.Eq(x => x.Longitude, longitude)
                );

                return await _weatherCollection.Find(filter)
                    .SortByDescending(x => x.RetrievedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather data for coordinates {Latitude}, {Longitude}", latitude, longitude);
                return null;
            }
        }

        public async Task<WeatherData?> GetWeatherDataByCityAsync(string city)
        {
            try
            {
                var filter = Builders<WeatherData>.Filter.And(
                    Builders<WeatherData>.Filter.Gte(x => x.ExpiresAt, DateTime.UtcNow),
                    Builders<WeatherData>.Filter.Eq(x => x.City, city)
                );

                return await _weatherCollection.Find(filter)
                    .SortByDescending(x => x.RetrievedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather data for city {City}", city);
                return null;
            }
        }

        public async Task SaveWeatherDataAsync(WeatherData weatherData)
        {
            try
            {
                await _weatherCollection.InsertOneAsync(weatherData);
                _logger.LogInformation("Weather data saved for coordinates {Latitude}, {Longitude}", 
                    weatherData.Latitude, weatherData.Longitude);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving weather data for coordinates {Latitude}, {Longitude}", 
                    weatherData.Latitude, weatherData.Longitude);
                throw;
            }
        }

        private void CreateIndexes()
        {
            try
            {
                var locationIndex = new CreateIndexModel<WeatherData>(
                    Builders<WeatherData>.IndexKeys
                        .Ascending(x => x.Latitude)
                        .Ascending(x => x.Longitude)
                        .Ascending(x => x.ExpiresAt)
                );

                var cityIndex = new CreateIndexModel<WeatherData>(
                    Builders<WeatherData>.IndexKeys
                        .Ascending(x => x.City)
                        .Ascending(x => x.ExpiresAt)
                );

                var expirationIndex = new CreateIndexModel<WeatherData>(
                    Builders<WeatherData>.IndexKeys.Ascending(x => x.ExpiresAt),
                    new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }
                );

                _weatherCollection.Indexes.CreateMany(new[] { locationIndex, cityIndex, expirationIndex });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create indexes");
            }
        }
    }
}
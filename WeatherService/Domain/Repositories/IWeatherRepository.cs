using WeatherService.Domain.Entities;

namespace WeatherService.Domain.Repositories
{
    /// <summary>
    /// Repository interface for weather data operations
    /// </summary>
    public interface IWeatherRepository
    {
        Task<WeatherData?> GetWeatherDataAsync(double latitude, double longitude);
        Task<WeatherData?> GetWeatherDataByCityAsync(string city);
        Task SaveWeatherDataAsync(WeatherData weatherData);
    }
}
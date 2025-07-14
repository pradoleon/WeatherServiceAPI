using WeatherService.Domain.Entities;

namespace WeatherService.Domain.Services
{
    /// <summary>
    /// Domain interface for external weather service integration
    /// </summary>
    public interface IExternalWeatherService
    {
        Task<WeatherData?> GetWeatherDataAsync(double latitude, double longitude, string? city = null);
    }
}
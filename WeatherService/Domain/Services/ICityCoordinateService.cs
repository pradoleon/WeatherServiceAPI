using WeatherService.Domain.ValueObjects;

namespace WeatherService.Domain.Services
{
    /// <summary>
    /// Domain service for city coordinate resolution
    /// </summary>
    public interface ICityCoordinateService
    {
        Coordinates? GetCoordinatesForCity(string cityName);
        bool IsCitySupported(string cityName);
        IEnumerable<string> GetSupportedCities();
    }
}
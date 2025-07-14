using WeatherService.Domain.Services;
using WeatherService.Domain.ValueObjects;

namespace WeatherService.Infraestructure.Services
{
    /// <summary>
    /// Implementation of city coordinate service with predefined city mappings
    /// </summary>
    public class CityCoordinateService : ICityCoordinateService
    {
        private static readonly Dictionary<string, (double Lat, double Lon)> CityCoordinates = new()
        {
            { "london", (51.5074, -0.1278) },
            { "paris", (48.8566, 2.3522) },
            { "tokyo", (35.6762, 139.6503) },
            { "new york", (40.7128, -74.0060) },
            { "madrid", (40.4168, -3.7038) },
            { "berlin", (52.5200, 13.4050) },
            { "rome", (41.9028, 12.4964) },
            { "sydney", (-33.8688, 151.2093) },
            { "moscow", (55.7558, 37.6176) },
            { "beijing", (39.9042, 116.4074) },
            { "mumbai", (19.0760, 72.8777) },
            { "cairo", (30.0444, 31.2357) },
            { "buenos aires", (-34.6118, -58.3960) },
            { "toronto", (43.6532, -79.3832) },
            { "amsterdam", (52.3676, 4.9041) }
        };

        private readonly ILogger<CityCoordinateService> _logger;

        public CityCoordinateService(ILogger<CityCoordinateService> logger)
        {
            _logger = logger;
        }

        public Coordinates? GetCoordinatesForCity(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                _logger.LogWarning("Empty city name provided");
                return null;
            }

            var normalizedCity = cityName.ToLowerInvariant().Trim();

            if (CityCoordinates.TryGetValue(normalizedCity, out var coordinates))
            {
                _logger.LogDebug("Found coordinates for city {City}: {Latitude}, {Longitude}",
                    cityName, coordinates.Lat, coordinates.Lon);
                return new Coordinates(coordinates.Lat, coordinates.Lon);
            }

            _logger.LogWarning("City {City} not found in supported cities", cityName);
            return null;
        }

        public bool IsCitySupported(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return false;

            var normalizedCity = cityName.ToLowerInvariant().Trim();
            return CityCoordinates.ContainsKey(normalizedCity);
        }

        public IEnumerable<string> GetSupportedCities()
        {
            return CityCoordinates.Keys.OrderBy(city => city);
        }
    }
}
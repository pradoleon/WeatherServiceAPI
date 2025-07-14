using MediatR;
using Microsoft.Extensions.Logging;
using WeatherService.Application.DTOs;
using WeatherService.Application.Queries;
using WeatherService.Domain.Entities;
using WeatherService.Domain.Repositories;
using WeatherService.Domain.Services;

namespace WeatherService.Application.Handlers
{
    /// <summary>
    /// Query handler for weather data retrieval by city name
    /// </summary>
    public class GetWeatherByCityHandler : IRequestHandler<GetWeatherByCityQuery, WeatherResponseDto?>
    {
        private readonly IWeatherRepository _weatherRepository;
        private readonly IExternalWeatherService _externalWeatherService;
        private readonly ICityCoordinateService _cityCoordinateService;
        private readonly ILogger<GetWeatherByCityHandler> _logger;

        public GetWeatherByCityHandler(
            IWeatherRepository weatherRepository,
            IExternalWeatherService externalWeatherService,
            ICityCoordinateService cityCoordinateService,
            ILogger<GetWeatherByCityHandler> logger)
        {
            _weatherRepository = weatherRepository;
            _externalWeatherService = externalWeatherService;
            _cityCoordinateService = cityCoordinateService;
            _logger = logger;
        }

        public async Task<WeatherResponseDto?> Handle(GetWeatherByCityQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.City))
                {
                    _logger.LogWarning("Empty city name provided in query");
                    return null;
                }

                var normalizedCity = request.City.ToLowerInvariant().Trim();

                // Check if city is supported
                if (!_cityCoordinateService.IsCitySupported(normalizedCity))
                {
                    _logger.LogWarning("City {City} is not supported", request.City);
                    return null;
                }

                // Try to get cached data first
                var cachedData = await _weatherRepository.GetWeatherDataByCityAsync(normalizedCity);
                if (cachedData != null && !cachedData.IsExpired)
                {
                    _logger.LogInformation("Returning cached weather data for city {City}", request.City);
                    return MapToDto(cachedData, "cache");
                }

                // Get coordinates for the city
                var coordinates = _cityCoordinateService.GetCoordinatesForCity(normalizedCity);
                if (coordinates == null)
                {
                    _logger.LogError("Unable to get coordinates for supported city {City}", request.City);
                    return null;
                }

                // Fetch from external API if not cached
                var externalData = await _externalWeatherService.GetWeatherDataAsync(
                    coordinates.Latitude,
                    coordinates.Longitude,
                    normalizedCity);

                if (externalData == null)
                {
                    _logger.LogWarning("Failed to retrieve weather data for city {City}", request.City);
                    return null;
                }

                // Save to cache
                await _weatherRepository.SaveWeatherDataAsync(externalData);

                return MapToDto(externalData, "external");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather data for city {City}", request.City);
                return null;
            }
        }

        private static WeatherResponseDto MapToDto(WeatherData weatherData, string source)
        {
            return new WeatherResponseDto(
                weatherData.Temperature,
                weatherData.WindDirection,
                weatherData.WindSpeed,
                weatherData.SunriseDateTime,
                source);
        }
    }
}
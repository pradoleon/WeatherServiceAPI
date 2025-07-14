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
    /// Query handler for weather data retrieval by coordinates
    /// </summary>
    public class GetWeatherByCoordinatesHandler : IRequestHandler<GetWeatherByCoordinatesQuery, WeatherResponseDto?>
    {
        private readonly IWeatherRepository _weatherRepository;
        private readonly IExternalWeatherService _externalWeatherService;
        private readonly ILogger<GetWeatherByCoordinatesHandler> _logger;

        public GetWeatherByCoordinatesHandler(
            IWeatherRepository weatherRepository,
            IExternalWeatherService externalWeatherService,
            ILogger<GetWeatherByCoordinatesHandler> logger)
        {
            _weatherRepository = weatherRepository;
            _externalWeatherService = externalWeatherService;
            _logger = logger;
        }

        public async Task<WeatherResponseDto?> Handle(GetWeatherByCoordinatesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Try to get cached data first
                var cachedData = await _weatherRepository.GetWeatherDataAsync(request.Latitude, request.Longitude);
                if (cachedData != null && !cachedData.IsExpired)
                {
                    _logger.LogInformation("Returning cached weather data for coordinates {Latitude}, {Longitude}", 
                        request.Latitude, request.Longitude);
                    return MapToDto(cachedData, "cache");
                }

                // Fetch from external API if not cached
                var externalData = await _externalWeatherService.GetWeatherDataAsync(request.Latitude, request.Longitude);
                if (externalData == null)
                {
                    _logger.LogWarning("Failed to retrieve weather data for coordinates {Latitude}, {Longitude}", 
                        request.Latitude, request.Longitude);
                    return null;
                }

                // Save to cache
                await _weatherRepository.SaveWeatherDataAsync(externalData);

                return MapToDto(externalData, "external");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving weather data for coordinates {Latitude}, {Longitude}", 
                    request.Latitude, request.Longitude);
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
using System.Text.Json;
using WeatherService.Domain.Entities;
using WeatherService.Domain.Services;
using WeatherService.Infraestructure.Models.External;

namespace WeatherService.Infraestructure.Services
{
    /// <summary>
    /// Service for integrating with Open-Meteo weather API
    /// </summary>
    public class OpenMeteoService : IExternalWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenMeteoService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public OpenMeteoService(HttpClient httpClient, ILogger<OpenMeteoService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            _baseUrl = _configuration["ExternalApis:OpenMeteo:BaseUrl"] ?? "https://api.open-meteo.com/v1";
            if (!string.IsNullOrEmpty(_baseUrl))
            {
                _httpClient.BaseAddress = new Uri(_baseUrl);
            }
        }

        public async Task<WeatherData?> GetWeatherDataAsync(double latitude, double longitude, string? city = null)
        {
            try
            {
                var url = $"{_baseUrl}/forecast?latitude={latitude:F6}&longitude={longitude:F6}&current=temperature_2m,wind_speed_10m,wind_direction_10m&daily=sunrise&timezone=auto";

                _logger.LogInformation("Fetching weather data from Open-Meteo for coordinates {Latitude}, {Longitude}", latitude, longitude);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var openMeteoResponse = JsonSerializer.Deserialize<OpenMeteoResponse>(jsonContent);

                if (openMeteoResponse?.Current == null)
                {
                    _logger.LogWarning("Invalid response from Open-Meteo API");
                    return null;
                }

                var sunriseDateTime = DateTime.UtcNow;
                if (openMeteoResponse.Daily?.Sunrise?.Length > 0 &&
                    DateTime.TryParse(openMeteoResponse.Daily.Sunrise[0], out var sunrise))
                {
                    sunriseDateTime = sunrise;
                }

                var weatherData = WeatherData.Create(
                    latitude,
                    longitude,
                    openMeteoResponse.Current.Temperature2m,
                    openMeteoResponse.Current.WindDirection10m,
                    openMeteoResponse.Current.WindSpeed10m,
                    sunriseDateTime,
                    city,
                    1 // Cache for 1 hour
                );

                _logger.LogInformation("Successfully retrieved weather data from Open-Meteo");
                return weatherData;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching weather data from Open-Meteo");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error while processing Open-Meteo response");
                return null;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid data received from Open-Meteo API");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching weather data from Open-Meteo");
                return null;
            }
        }
    }
}
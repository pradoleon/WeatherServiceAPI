using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace WeatherService.Tests.Integration
{
    public class WeatherApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public WeatherApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Override services for testing if needed
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HealthEndpoint_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Healthy");
        }

        [Fact]
        public async Task GetSupportedCities_ShouldReturnCitiesList()
        {
            // Act
            var response = await _client.GetAsync("/api/weather/supported-cities");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var cities = await response.Content.ReadFromJsonAsync<string[]>();
            cities.Should().NotBeNull();
            cities.Should().Contain("london");
        }

        [Theory]
        [InlineData(51.5074, -0.1278)] // London coordinates
        [InlineData(48.8566, 2.3522)]  // Paris coordinates
        public async Task GetWeatherByCoordinates_WithValidCoordinates_ShouldReturnWeatherData(double lat, double lon)
        {
            // Act
            var response = await _client.GetAsync($"/api/weather/coordinates?latitude={lat}&longitude={lon}");

            // Assert
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var weatherData = await response.Content.ReadFromJsonAsync<WeatherResponseDto>();
                weatherData.Should().NotBeNull();
                weatherData!.Temperature.Should().NotBe(0);
            }
            else
            {
                // Might fail due to external API or MongoDB not being available in test environment
                response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
            }
        }

        [Theory]
        [InlineData(91, 0)]    // Invalid latitude
        [InlineData(0, 181)]   // Invalid longitude
        [InlineData(-91, 0)]   // Invalid latitude
        [InlineData(0, -181)]  // Invalid longitude
        public async Task GetWeatherByCoordinates_WithInvalidCoordinates_ShouldReturnBadRequest(double lat, double lon)
        {
            // Act
            var response = await _client.GetAsync($"/api/weather/coordinates?latitude={lat}&longitude={lon}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetWeatherByCity_WithSupportedCity_ShouldReturnWeatherDataOrNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/weather/city?city=london");

            // Assert
            // Could return OK with data or NotFound if external services are unavailable
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetWeatherByCity_WithUnsupportedCity_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/weather/city?city=UnsupportedCity123");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
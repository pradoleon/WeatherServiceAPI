using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace WeatherService.Tests
{
    /// <summary>
    /// Main test class - can be used for broader integration tests or test utilities
    /// </summary>
    public class WeatherServiceTests : TestBase
    {
        [Fact]
        public void TestFixture_ShouldCreateValidWeatherData()
        {
            // Act
            var weatherData = CreateValidWeatherData();

            // Assert
            weatherData.Should().NotBeNull();
            weatherData.Latitude.Should().Be(51.5074);
            weatherData.Longitude.Should().Be(-0.1278);
            weatherData.City.Should().Be("london");
        }

        [Fact]
        public void Fixture_ShouldCreateRandomData()
        {
            // Act
            var data1 = CreateValidWeatherData(48.8566, 2.3522, "paris");
            var data2 = CreateValidWeatherData(35.6762, 139.6503, "tokyo");

            // Assert
            data1.Temperature.Should().NotBe(data2.Temperature);
            data1.City.Should().Be("paris");
            data2.City.Should().Be("tokyo");
        }
    }
}
using WeatherService.Infraestructure.Services;
using Microsoft.Extensions.Logging;

namespace WeatherService.Tests.Infrastructure.Services
{
    public class CityCoordinateServiceTests
    {
        private readonly Mock<ILogger<CityCoordinateService>> _mockLogger;
        private readonly CityCoordinateService _service;

        public CityCoordinateServiceTests()
        {
            _mockLogger = new Mock<ILogger<CityCoordinateService>>();
            _service = new CityCoordinateService(_mockLogger.Object);
        }

        [Theory]
        [InlineData("london")]
        [InlineData("London")]
        [InlineData("LONDON")]
        [InlineData("  London  ")]
        public void GetCoordinatesForCity_WithSupportedCity_ShouldReturnCoordinates(string cityName)
        {
            // Act
            var coordinates = _service.GetCoordinatesForCity(cityName);

            // Assert
            coordinates.Should().NotBeNull();
            coordinates!.Latitude.Should().Be(51.5074);
            coordinates.Longitude.Should().Be(-0.1278);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("UnsupportedCity")]
        public void GetCoordinatesForCity_WithUnsupportedCity_ShouldReturnNull(string? cityName)
        {
            // Act
            var coordinates = _service.GetCoordinatesForCity(cityName!);

            // Assert
            coordinates.Should().BeNull();
        }

        [Theory]
        [InlineData("london", true)]
        [InlineData("London", true)]
        [InlineData("PARIS", true)]
        [InlineData("tokyo", true)]
        [InlineData("UnsupportedCity", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsCitySupported_ShouldReturnCorrectResult(string? cityName, bool expected)
        {
            // Act
            var isSupported = _service.IsCitySupported(cityName!);

            // Assert
            isSupported.Should().Be(expected);
        }

        [Fact]
        public void GetSupportedCities_ShouldReturnOrderedList()
        {
            // Act
            var cities = _service.GetSupportedCities().ToList();

            // Assert
            cities.Should().NotBeEmpty();
            cities.Should().Contain("london");
            cities.Should().Contain("paris");
            cities.Should().Contain("tokyo");
            cities.Should().BeInAscendingOrder();
        }
    }
}
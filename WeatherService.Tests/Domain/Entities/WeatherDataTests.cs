using WeatherService.Domain.Entities;

namespace WeatherService.Tests.Domain.Entities
{
    public class WeatherDataTests
    {
        private readonly Fixture _fixture;

        public WeatherDataTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void Create_WithValidData_ShouldCreateWeatherData()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;
            var temperature = 20.5;
            var windDirection = 180.0;
            var windSpeed = 10.5;
            var sunriseDateTime = DateTime.UtcNow.AddHours(6);
            var city = "london";

            // Act
            var weatherData = WeatherData.Create(
                latitude, longitude, temperature, windDirection, windSpeed, sunriseDateTime, city);

            // Assert
            weatherData.Should().NotBeNull();
            weatherData.Latitude.Should().Be(latitude);
            weatherData.Longitude.Should().Be(longitude);
            weatherData.Temperature.Should().Be(temperature);
            weatherData.WindDirection.Should().Be(windDirection);
            weatherData.WindSpeed.Should().Be(windSpeed);
            weatherData.SunriseDateTime.Should().Be(sunriseDateTime);
            weatherData.City.Should().Be(city.ToLowerInvariant());
            weatherData.RetrievedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            weatherData.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(-91, 0)] // Invalid latitude
        [InlineData(91, 0)]  // Invalid latitude
        [InlineData(0, -181)] // Invalid longitude
        [InlineData(0, 181)]  // Invalid longitude
        public void Create_WithInvalidCoordinates_ShouldThrowArgumentException(double latitude, double longitude)
        {
            // Arrange
            var temperature = 20.5;
            var windDirection = 180.0;
            var windSpeed = 10.5;
            var sunriseDateTime = DateTime.UtcNow;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                WeatherData.Create(latitude, longitude, temperature, windDirection, windSpeed, sunriseDateTime));
        }

        [Theory]
        [InlineData(-1)]   // Negative wind direction
        [InlineData(361)]  // Wind direction > 360
        public void Create_WithInvalidWindDirection_ShouldThrowArgumentException(double windDirection)
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;
            var temperature = 20.5;
            var windSpeed = 10.5;
            var sunriseDateTime = DateTime.UtcNow;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                WeatherData.Create(latitude, longitude, temperature, windDirection, windSpeed, sunriseDateTime));
        }

        [Fact]
        public void Create_WithNegativeWindSpeed_ShouldThrowArgumentException()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;
            var temperature = 20.5;
            var windDirection = 180.0;
            var windSpeed = -5.0; // Negative wind speed
            var sunriseDateTime = DateTime.UtcNow;

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                WeatherData.Create(latitude, longitude, temperature, windDirection, windSpeed, sunriseDateTime));
        }

        [Fact]
        public void IsExpired_WithFutureExpiryDate_ShouldReturnFalse()
        {
            // Arrange
            var weatherData = WeatherData.Create(51.5074, -0.1278, 20.5, 180.0, 10.5, DateTime.UtcNow);

            // Act
            var isExpired = weatherData.IsExpired;

            // Assert
            isExpired.Should().BeFalse();
        }

        [Fact]
        public void IsExpired_WithPastExpiryDate_ShouldReturnTrue()
        {
            // Arrange
            var weatherData = WeatherData.Create(51.5074, -0.1278, 20.5, 180.0, 10.5, DateTime.UtcNow, cacheHours: -1);

            // Act
            var isExpired = weatherData.IsExpired;

            // Assert
            isExpired.Should().BeTrue();
        }

        [Fact]
        public void MatchesLocation_WithSameCoordinates_ShouldReturnTrue()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;
            var weatherData = WeatherData.Create(latitude, longitude, 20.5, 180.0, 10.5, DateTime.UtcNow);

            // Act
            var matches = weatherData.MatchesLocation(latitude, longitude);

            // Assert
            matches.Should().BeTrue();
        }

        [Fact]
        public void MatchesLocation_WithDifferentCoordinates_ShouldReturnFalse()
        {
            // Arrange
            var weatherData = WeatherData.Create(51.5074, -0.1278, 20.5, 180.0, 10.5, DateTime.UtcNow);

            // Act
            var matches = weatherData.MatchesLocation(52.5074, -1.1278); // Different coordinates

            // Assert
            matches.Should().BeFalse();
        }

        [Fact]
        public void MatchesLocation_WithinTolerance_ShouldReturnTrue()
        {
            // Arrange
            var weatherData = WeatherData.Create(51.5074, -0.1278, 20.5, 180.0, 10.5, DateTime.UtcNow);

            // Act
            var matches = weatherData.MatchesLocation(51.5075, -0.1279, 0.01); // Within tolerance

            // Assert
            matches.Should().BeTrue();
        }
    }
}
using WeatherService.Domain.ValueObjects;

namespace WeatherService.Tests.Domain.ValueObjects
{
    public class CoordinatesTests
    {
        [Fact]
        public void Constructor_WithValidCoordinates_ShouldCreateInstance()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;

            // Act
            var coordinates = new Coordinates(latitude, longitude);

            // Assert
            coordinates.Latitude.Should().Be(latitude);
            coordinates.Longitude.Should().Be(longitude);
        }

        [Theory]
        [InlineData(-91, 0)]   // Invalid latitude
        [InlineData(91, 0)]    // Invalid latitude
        [InlineData(0, -181)]  // Invalid longitude
        [InlineData(0, 181)]   // Invalid longitude
        public void Constructor_WithInvalidCoordinates_ShouldThrowArgumentException(double latitude, double longitude)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Coordinates(latitude, longitude));
        }

        [Fact]
        public void IsNear_WithSameCoordinates_ShouldReturnTrue()
        {
            // Arrange
            var coordinates1 = new Coordinates(51.5074, -0.1278);
            var coordinates2 = new Coordinates(51.5074, -0.1278);

            // Act
            var isNear = coordinates1.IsNear(coordinates2);

            // Assert
            isNear.Should().BeTrue();
        }

        [Fact]
        public void IsNear_WithinTolerance_ShouldReturnTrue()
        {
            // Arrange
            var coordinates1 = new Coordinates(51.5074, -0.1278);
            var coordinates2 = new Coordinates(51.5075, -0.1279);

            // Act
            var isNear = coordinates1.IsNear(coordinates2, 0.01);

            // Assert
            isNear.Should().BeTrue();
        }

        [Fact]
        public void IsNear_OutsideTolerance_ShouldReturnFalse()
        {
            // Arrange
            var coordinates1 = new Coordinates(51.5074, -0.1278);
            var coordinates2 = new Coordinates(52.5074, -1.1278);

            // Act
            var isNear = coordinates1.IsNear(coordinates2);

            // Assert
            isNear.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithSameValues_ShouldReturnTrue()
        {
            // Arrange
            var coordinates1 = new Coordinates(51.5074, -0.1278);
            var coordinates2 = new Coordinates(51.5074, -0.1278);

            // Act
            var areEqual = coordinates1.Equals(coordinates2);

            // Assert
            areEqual.Should().BeTrue();
        }

        [Fact]
        public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
        {
            // Arrange
            var coordinates1 = new Coordinates(51.5074, -0.1278);
            var coordinates2 = new Coordinates(51.5074, -0.1278);

            // Act
            var hash1 = coordinates1.GetHashCode();
            var hash2 = coordinates2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }
    }
}
using WeatherService.Application.Handlers;
using WeatherService.Application.Queries;
using WeatherService.Domain.Repositories;
using WeatherService.Domain.Services;
using WeatherService.Domain.ValueObjects;

namespace WeatherService.Tests.Application.Handlers
{
    public class GetWeatherByCityHandlerTests
    {
        private readonly Mock<IWeatherRepository> _mockRepository;
        private readonly Mock<IExternalWeatherService> _mockExternalService;
        private readonly Mock<ICityCoordinateService> _mockCityService;
        private readonly Mock<ILogger<GetWeatherByCityHandler>> _mockLogger;
        private readonly GetWeatherByCityHandler _handler;

        public GetWeatherByCityHandlerTests()
        {
            _mockRepository = new Mock<IWeatherRepository>();
            _mockExternalService = new Mock<IExternalWeatherService>();
            _mockCityService = new Mock<ICityCoordinateService>();
            _mockLogger = new Mock<ILogger<GetWeatherByCityHandler>>();
            _handler = new GetWeatherByCityHandler(
                _mockRepository.Object,
                _mockExternalService.Object,
                _mockCityService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_WithSupportedCityAndCachedData_ShouldReturnCachedResult()
        {
            // Arrange
            var query = new GetWeatherByCityQuery("London");
            var coordinates = new Coordinates(51.5074, -0.1278);
            var cachedData = WeatherData.Create(
                coordinates.Latitude, coordinates.Longitude, 20.5, 180, 10.5, DateTime.UtcNow.AddHours(6), "london");

            _mockCityService.Setup(x => x.IsCitySupported("london")).Returns(true);
            _mockRepository.Setup(x => x.GetWeatherDataByCityAsync("london"))
                .ReturnsAsync(cachedData);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Source.Should().Be("cache");
            result.Temperature.Should().Be(cachedData.Temperature);
        }

        [Fact]
        public async Task Handle_WithUnsupportedCity_ShouldReturnNull()
        {
            // Arrange
            var query = new GetWeatherByCityQuery("UnsupportedCity");

            _mockCityService.Setup(x => x.IsCitySupported("unsupportedcity")).Returns(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(x => x.GetWeatherDataByCityAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithEmptyCity_ShouldReturnNull()
        {
            // Arrange
            var query = new GetWeatherByCityQuery("");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WithWhitespaceCity_ShouldReturnNull()
        {
            // Arrange
            var query = new GetWeatherByCityQuery("   ");

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_WithoutCachedData_ShouldFetchFromExternalApi()
        {
            // Arrange
            var query = new GetWeatherByCityQuery("London");
            var coordinates = new Coordinates(51.5074, -0.1278);
            var externalData = WeatherData.Create(
                coordinates.Latitude, coordinates.Longitude, 18.5, 90, 8.0, DateTime.UtcNow.AddHours(6), "london");

            _mockCityService.Setup(x => x.IsCitySupported("london")).Returns(true);
            _mockRepository.Setup(x => x.GetWeatherDataByCityAsync("london"))
                .ReturnsAsync((WeatherData?)null);
            _mockCityService.Setup(x => x.GetCoordinatesForCity("london")).Returns(coordinates);
            _mockExternalService.Setup(x => x.GetWeatherDataAsync(coordinates.Latitude, coordinates.Longitude, "london"))
                .ReturnsAsync(externalData);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Source.Should().Be("external");
            result.Temperature.Should().Be(externalData.Temperature);

            _mockRepository.Verify(x => x.SaveWeatherDataAsync(externalData), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenCoordinatesServiceReturnsNull_ShouldReturnNull()
        {
            // Arrange
            var query = new GetWeatherByCityQuery("London");

            _mockCityService.Setup(x => x.IsCitySupported("london")).Returns(true);
            _mockRepository.Setup(x => x.GetWeatherDataByCityAsync("london"))
                .ReturnsAsync((WeatherData?)null);
            _mockCityService.Setup(x => x.GetCoordinatesForCity("london")).Returns((Coordinates?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
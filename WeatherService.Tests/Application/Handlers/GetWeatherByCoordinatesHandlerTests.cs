using WeatherService.Application.Handlers;
using WeatherService.Application.Queries;
using WeatherService.Domain.Repositories;
using WeatherService.Domain.Services;

namespace WeatherService.Tests.Application.Handlers
{
    public class GetWeatherByCoordinatesHandlerTests
    {
        private readonly Mock<IWeatherRepository> _mockRepository;
        private readonly Mock<IExternalWeatherService> _mockExternalService;
        private readonly Mock<ILogger<GetWeatherByCoordinatesHandler>> _mockLogger;
        private readonly GetWeatherByCoordinatesHandler _handler;
        private readonly Fixture _fixture;

        public GetWeatherByCoordinatesHandlerTests()
        {
            _mockRepository = new Mock<IWeatherRepository>();
            _mockExternalService = new Mock<IExternalWeatherService>();
            _mockLogger = new Mock<ILogger<GetWeatherByCoordinatesHandler>>();
            _handler = new GetWeatherByCoordinatesHandler(
                _mockRepository.Object,
                _mockExternalService.Object,
                _mockLogger.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Handle_WithCachedData_ShouldReturnCachedResult()
        {
            // Arrange
            var query = new GetWeatherByCoordinatesQuery(51.5074, -0.1278);
            var cachedData = WeatherData.Create(
                query.Latitude, query.Longitude, 20.5, 180, 10.5, DateTime.UtcNow.AddHours(6));

            _mockRepository.Setup(x => x.GetWeatherDataAsync(query.Latitude, query.Longitude))
                .ReturnsAsync(cachedData);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Source.Should().Be("cache");
            result.Temperature.Should().Be(cachedData.Temperature);
            result.WindDirection.Should().Be(cachedData.WindDirection);
            result.WindSpeed.Should().Be(cachedData.WindSpeed);
            result.SunriseDateTime.Should().Be(cachedData.SunriseDateTime);

            _mockExternalService.Verify(x => x.GetWeatherDataAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithExpiredCachedData_ShouldFetchFromExternalApi()
        {
            // Arrange
            var query = new GetWeatherByCoordinatesQuery(51.5074, -0.1278);
            var expiredData = WeatherData.Create(
                query.Latitude, query.Longitude, 20.5, 180, 10.5, DateTime.UtcNow, cacheHours: -1);
            var freshData = WeatherData.Create(
                query.Latitude, query.Longitude, 22.0, 270, 15.0, DateTime.UtcNow.AddHours(6));

            _mockRepository.Setup(x => x.GetWeatherDataAsync(query.Latitude, query.Longitude))
                .ReturnsAsync(expiredData);
            _mockExternalService.Setup(x => x.GetWeatherDataAsync(query.Latitude, query.Longitude, null))
                .ReturnsAsync(freshData);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Source.Should().Be("external");
            result.Temperature.Should().Be(freshData.Temperature);

            _mockRepository.Verify(x => x.SaveWeatherDataAsync(freshData), Times.Once);
        }

        [Fact]
        public async Task Handle_WithoutCachedData_ShouldFetchFromExternalApi()
        {
            // Arrange
            var query = new GetWeatherByCoordinatesQuery(51.5074, -0.1278);
            var externalData = WeatherData.Create(
                query.Latitude, query.Longitude, 18.5, 90, 8.0, DateTime.UtcNow.AddHours(6));

            _mockRepository.Setup(x => x.GetWeatherDataAsync(query.Latitude, query.Longitude))
                .ReturnsAsync((WeatherData?)null);
            _mockExternalService.Setup(x => x.GetWeatherDataAsync(query.Latitude, query.Longitude, null))
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
        public async Task Handle_WhenExternalServiceReturnsNull_ShouldReturnNull()
        {
            // Arrange
            var query = new GetWeatherByCoordinatesQuery(51.5074, -0.1278);

            _mockRepository.Setup(x => x.GetWeatherDataAsync(query.Latitude, query.Longitude))
                .ReturnsAsync((WeatherData?)null);
            _mockExternalService.Setup(x => x.GetWeatherDataAsync(query.Latitude, query.Longitude, null))
                .ReturnsAsync((WeatherData?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(x => x.SaveWeatherDataAsync(It.IsAny<WeatherData>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenExceptionOccurs_ShouldReturnNull()
        {
            // Arrange
            var query = new GetWeatherByCoordinatesQuery(51.5074, -0.1278);

            _mockRepository.Setup(x => x.GetWeatherDataAsync(query.Latitude, query.Longitude))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}
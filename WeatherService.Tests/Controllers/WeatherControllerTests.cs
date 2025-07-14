using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherService.Application.Queries;
using WeatherService.Controllers;
using WeatherService.Domain.Services;

namespace WeatherService.Tests.Controllers
{
    public class WeatherControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ICityCoordinateService> _mockCityService;
        private readonly Mock<ILogger<WeatherController>> _mockLogger;
        private readonly WeatherController _controller;

        public WeatherControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockCityService = new Mock<ICityCoordinateService>();
            _mockLogger = new Mock<ILogger<WeatherController>>();
            _controller = new WeatherController(_mockMediator.Object, _mockCityService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetWeatherByCoordinates_WithValidCoordinates_ShouldReturnOkResult()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;
            var expectedResponse = new WeatherResponseDto(20.5, 180, 10.5, DateTime.UtcNow.AddHours(6), "external");

            _mockMediator.Setup(x => x.Send(It.IsAny<GetWeatherByCoordinatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetWeatherByCoordinates(latitude, longitude);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task GetWeatherByCoordinates_WhenMediatorReturnsNull_ShouldReturnNotFound()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;

            _mockMediator.Setup(x => x.Send(It.IsAny<GetWeatherByCoordinatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WeatherResponseDto?)null);

            // Act
            var result = await _controller.GetWeatherByCoordinates(latitude, longitude);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetWeatherByCoordinates_WhenExceptionOccurs_ShouldReturnInternalServerError()
        {
            // Arrange
            var latitude = 51.5074;
            var longitude = -0.1278;

            _mockMediator.Setup(x => x.Send(It.IsAny<GetWeatherByCoordinatesQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act
            var result = await _controller.GetWeatherByCoordinates(latitude, longitude);

            // Assert
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetWeatherByCity_WithValidCity_ShouldReturnOkResult()
        {
            // Arrange
            var city = "London";
            var expectedResponse = new WeatherResponseDto(18.5, 90, 8.0, DateTime.UtcNow.AddHours(6), "cache");

            _mockMediator.Setup(x => x.Send(It.IsAny<GetWeatherByCityQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetWeatherByCity(city);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(expectedResponse);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetWeatherByCity_WithInvalidCity_ShouldReturnBadRequest(string city)
        {
            // Act
            var result = await _controller.GetWeatherByCity(city);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetWeatherByCity_WhenMediatorReturnsNull_ShouldReturnNotFound()
        {
            // Arrange
            var city = "UnsupportedCity";

            _mockMediator.Setup(x => x.Send(It.IsAny<GetWeatherByCityQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WeatherResponseDto?)null);

            // Act
            var result = await _controller.GetWeatherByCity(city);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public void GetSupportedCities_ShouldReturnOkResultWithCities()
        {
            // Arrange
            var supportedCities = new[] { "london", "paris", "tokyo" };
            _mockCityService.Setup(x => x.GetSupportedCities()).Returns(supportedCities);

            // Act
            var result = _controller.GetSupportedCities();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(supportedCities);
        }
    }
}
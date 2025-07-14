using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WeatherService.Application.DTOs;
using WeatherService.Application.Queries;
using WeatherService.Domain.Services;
using WeatherService.Infraestructure.Services;

namespace WeatherService.Controllers
{
    /// <summary>
    /// Weather API controller using CQRS pattern
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WeatherController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICityCoordinateService _cityCoordinateService;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(IMediator mediator, ICityCoordinateService cityCoordinateService, ILogger<WeatherController> logger)
        {
            _mediator = mediator;
            _cityCoordinateService = cityCoordinateService;
            _logger = logger;
        }

        /// <summary>
        /// Gets weather data by geographical coordinates
        /// </summary>
        [HttpGet("coordinates")]
        [ProducesResponseType(typeof(WeatherResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetWeatherByCoordinates(
            [FromQuery] [Required] [Range(-90, 90)] double latitude,
            [FromQuery] [Required] [Range(-180, 180)] double longitude)
        {
            try
            {
                var query = new GetWeatherByCoordinatesQuery(latitude, longitude);
                var result = await _mediator.Send(query);
                
                if (result == null)
                {
                    return NotFound("Weather data not available for the specified coordinates");
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing weather request");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Gets weather data by city name
        /// </summary>
        [HttpGet("city")]
        [ProducesResponseType(typeof(WeatherResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetWeatherByCity([FromQuery] [Required] string city)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(city))
                {
                    return BadRequest("City name is required");
                }

                var query = new GetWeatherByCityQuery(city);
                var result = await _mediator.Send(query);
                
                if (result == null)
                {
                    return NotFound("Weather data not available for the specified city or city not supported");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing weather request for city {City}", city);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        /// <summary>
        /// Gets list of supported cities
        /// </summary>
        /// <returns>List of supported city names</returns>
        [HttpGet("supported-cities")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public IActionResult GetSupportedCities()
        {
            var cities = _cityCoordinateService.GetSupportedCities();
            return Ok(cities);
        }
    }
}
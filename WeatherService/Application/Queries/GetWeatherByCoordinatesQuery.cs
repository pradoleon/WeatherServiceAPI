using MediatR;
using WeatherService.Application.DTOs;

namespace WeatherService.Application.Queries
{
    /// <summary>
    /// Query for retrieving weather data by coordinates
    /// </summary>
    public record GetWeatherByCoordinatesQuery(double Latitude, double Longitude) : IRequest<WeatherResponseDto?>;
}
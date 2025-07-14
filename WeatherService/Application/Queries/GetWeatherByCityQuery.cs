using MediatR;
using WeatherService.Application.DTOs;

namespace WeatherService.Application.Queries
{
    /// <summary>
    /// Query for retrieving weather data by city name
    /// </summary>
    public record GetWeatherByCityQuery(string City) : IRequest<WeatherResponseDto?>;
}
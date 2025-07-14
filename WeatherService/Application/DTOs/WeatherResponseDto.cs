namespace WeatherService.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for weather responses
    /// </summary>
    public record WeatherResponseDto(
        double Temperature,
        double WindDirection,
        double WindSpeed,
        DateTime SunriseDateTime,
        string Source);
}
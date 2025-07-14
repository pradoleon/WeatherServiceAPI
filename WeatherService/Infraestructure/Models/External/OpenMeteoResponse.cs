using System.Text.Json.Serialization;

namespace WeatherService.Infraestructure.Models.External
{
    /// <summary>
    /// Response model for Open-Meteo API
    /// </summary>
    public class OpenMeteoResponse
    {
        [JsonPropertyName("current")]
        public Current? Current { get; set; }

        [JsonPropertyName("daily")]
        public Daily? Daily { get; set; }
    }

    public class Current
    {
        [JsonPropertyName("temperature_2m")]
        public double Temperature2m { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed10m { get; set; }

        [JsonPropertyName("wind_direction_10m")]
        public double WindDirection10m { get; set; }
    }

    public class Daily
    {
        [JsonPropertyName("sunrise")]
        public string[]? Sunrise { get; set; }
    }
}
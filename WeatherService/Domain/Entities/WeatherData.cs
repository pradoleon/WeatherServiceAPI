using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WeatherService.Domain.Entities
{
    /// <summary>
    /// Domain entity representing weather data with business rules
    /// </summary>
    public class WeatherData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; private set; }

        [BsonElement("latitude")]
        public double Latitude { get; private set; }

        [BsonElement("longitude")]
        public double Longitude { get; private set; }

        [BsonElement("city")]
        public string? City { get; private set; }

        [BsonElement("temperature")]
        public double Temperature { get; private set; }

        [BsonElement("windDirection")]
        public double WindDirection { get; private set; }

        [BsonElement("windSpeed")]
        public double WindSpeed { get; private set; }

        [BsonElement("sunriseDateTime")]
        public DateTime SunriseDateTime { get; private set; }

        [BsonElement("retrievedAt")]
        public DateTime RetrievedAt { get; private set; }

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; private set; }

        // Private constructor for MongoDB
        private WeatherData() { }

        // Factory method with business rules
        public static WeatherData Create(
            double latitude,
            double longitude,
            double temperature,
            double windDirection,
            double windSpeed,
            DateTime sunriseDateTime,
            string? city = null,
            int cacheHours = 1)
        {
            if (!IsValidCoordinate(latitude, longitude))
                throw new ArgumentException("Invalid coordinates provided");

            if (windDirection < 0 || windDirection > 360)
                throw new ArgumentException("Wind direction must be between 0 and 360 degrees");

            if (windSpeed < 0)
                throw new ArgumentException("Wind speed cannot be negative");

            return new WeatherData
            {
                Latitude = latitude,
                Longitude = longitude,
                City = city?.ToLowerInvariant(),
                Temperature = temperature,
                WindDirection = windDirection,
                WindSpeed = windSpeed,
                SunriseDateTime = sunriseDateTime,
                RetrievedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(cacheHours)
            };
        }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public bool MatchesLocation(double latitude, double longitude, double tolerance = 0.001)
        {
            return Math.Abs(Latitude - latitude) < tolerance && 
                   Math.Abs(Longitude - longitude) < tolerance;
        }

        private static bool IsValidCoordinate(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
        }
    }
}
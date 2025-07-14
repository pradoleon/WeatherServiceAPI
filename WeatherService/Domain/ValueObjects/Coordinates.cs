namespace WeatherService.Domain.ValueObjects
{
    /// <summary>
    /// Value object representing geographical coordinates
    /// </summary>
    public record Coordinates
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public Coordinates(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("Latitude must be between -90 and 90 degrees", nameof(latitude));
            
            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("Longitude must be between -180 and 180 degrees", nameof(longitude));

            Latitude = latitude;
            Longitude = longitude;
        }

        public bool IsNear(Coordinates other, double toleranceInDegrees = 0.001)
        {
            return Math.Abs(Latitude - other.Latitude) < toleranceInDegrees &&
                   Math.Abs(Longitude - other.Longitude) < toleranceInDegrees;
        }
    }
}
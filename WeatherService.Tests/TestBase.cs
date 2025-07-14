using AutoFixture;

namespace WeatherService.Tests
{
    public abstract class TestBase
    {
        protected readonly Fixture Fixture;

        protected TestBase()
        {
            Fixture = new Fixture();
            Fixture.Behaviors.Remove(new ThrowingRecursionBehavior());
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        protected WeatherData CreateValidWeatherData(
            double latitude = 51.5074,
            double longitude = -0.1278,
            string? city = "london")
        {
            return WeatherData.Create(
                latitude,
                longitude,
                Fixture.Create<double>() % 50, // Temperature between -50 and 50
                Fixture.Create<double>() % 360, // Wind direction 0-360
                Math.Abs(Fixture.Create<double>() % 50), // Positive wind speed
                DateTime.UtcNow.AddHours(Fixture.Create<int>() % 12), // Sunrise within 12 hours
                city);
        }
    }
}
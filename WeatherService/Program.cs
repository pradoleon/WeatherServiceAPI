using MediatR;
using MongoDB.Driver;
using WeatherService.Application.Handlers;
using WeatherService.Domain.Repositories;
using WeatherService.Domain.Services;
using System.Reflection;
using WeatherService.Infraestructure.Repositories;
using WeatherService.Infraestructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// MediatR for CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetWeatherByCoordinatesHandler).Assembly));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var swaggerConfig = builder.Configuration.GetSection("Swagger");
    c.SwaggerDoc(swaggerConfig["Version"], new()
    {
        Title = swaggerConfig["Title"],
        Version = swaggerConfig["Version"],
        Description = swaggerConfig["Description"],
        Contact = new()
        {
            Name = swaggerConfig.GetSection("Contact")["Name"],
            Email = swaggerConfig.GetSection("Contact")["Email"]
        }
    });

    // Include XML comments for better Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// MongoDB configuration
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDB");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("MongoDB connection string is not configured.");
    }
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["Database:Name"];
    if (string.IsNullOrEmpty(databaseName))
    {
        throw new InvalidOperationException("Database name is not configured.");
    }
    return client.GetDatabase(databaseName);
});

// HTTP Client for external API calls
builder.Services.AddHttpClient<IExternalWeatherService, OpenMeteoService>();

// Register repositories and services
builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<IExternalWeatherService, OpenMeteoService>();
builder.Services.AddScoped<ICityCoordinateService, CityCoordinateService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        var swaggerConfig = builder.Configuration.GetSection("Swagger");
        c.SwaggerEndpoint(swaggerConfig["Endpoint"], $"{swaggerConfig["Title"]} {swaggerConfig["Version"]}");
        c.RoutePrefix = swaggerConfig["RoutePrefix"];
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
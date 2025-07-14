# Weather Service API

A comprehensive REST API for weather data with MongoDB caching, built using Clean Architecture and CQRS patterns. This educational project demonstrates modern .NET development practices and production-ready code structure.

## 🌟 Features

- **Clean Architecture**: Separation of concerns with Domain, Application, Infrastructure, and Presentation layers
- **CQRS Pattern**: Using MediatR for command/query separation
- **MongoDB Caching**: Intelligent caching to minimize external API calls
- **OpenAPI/Swagger Documentation**: Self-documenting API with interactive testing
- **Comprehensive Testing**: Unit, integration, and domain tests
- **External API Integration**: Real weather data from Open-Meteo API
- **Production-Ready**: Error handling, logging and validation

## 🏗️ Architecture
WeatherService/
├── Controllers/           # API endpoints (Presentation Layer)
├── Application/           # Use cases and handlers (Application Layer)
│   ├── DTOs/              # Data transfer objects
│   ├── Handlers/          # CQRS command/query handlers
│   └── Queries/           # Query definitions
├── Domain/                # Business logic (Domain Layer)
│   ├── Entities/          # Domain entities with business rules
│   ├── ValueObjects/      # Immutable value objects
│   ├── Services/          # Domain service interfaces
│   └── Repositories/      # Repository abstractions
├── Infrastructure/        # External concerns (Infrastructure Layer)
│   ├── Models/            # External API models
│   ├── Repositories/      # Data access implementations
│   └── Services/          # External service integrations
└── Tests/                 # Comprehensive test suite

## 🚀 Quick Start

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [MongoDB](https://www.mongodb.com/try/download/community) (running locally)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
git clone <repository-url> cd WeatherService


2. **Install MongoDB**
   - Download and install MongoDB Community Edition
   - Start MongoDB service:
     ```bash
     # Windows (if installed as service)
     net start MongoDB
     
     # macOS (with Homebrew)
     brew services start mongodb-community
     
     # Linux (systemd)
     sudo systemctl start mongod
     ```

3. **Restore NuGet packages**
 dotnet restore

4. **Build the solution**
dotnet build

5. **Run the application**
cd WeatherService dotnet run

6. **Access the API**
   - Swagger UI: `https://localhost:7053/swagger`
   - Health Check: `https://localhost:7053/health`

## 📝 API Endpoints

### Weather Data

#### Get Weather by Coordinates
GET /api/weather/coordinates?latitude={lat}&longitude={lon}

**Example:**
curl "https://localhost:7053/api/weather/coordinates?latitude=51.5074&longitude=-0.1278"
**Response:**
{ "temperature": 20.5, "windDirection": 180.0, "windSpeed": 10.5, "sunriseDateTime": "2024-01-15T06:30:00Z", "source": "external" }

#### Get Weather by City
GET /api/weather/city?city={cityName}
**Example:**
curl "https://localhost:7053/api/weather/city?city=london"

#### Get Supported Cities
GET /api/weather/supported-cities

**Supported Cities:**
- london, paris, tokyo, new york, madrid, berlin, rome, sydney, moscow, beijing, mumbai, cairo, buenos aires, toronto, amsterdam

### System Endpoints

#### Health Check
GET /health

## 🔧 Configuration

### Application Settings

Update `appsettings.json` or use environment variables:
{ "ConnectionStrings": { "MongoDB": "mongodb://localhost:27017" }, "Database": { "Name": "WeatherServiceDb" }, "ExternalApis": { "OpenMeteo": { "BaseUrl": "https://api.open-meteo.com/v1" } } }

### Environment Variables

For production, use environment variables:
ASPNETCORE_ENVIRONMENT=Production ConnectionStrings__MongoDB=mongodb://your-mongo-server:27017 Database__Name=WeatherServiceProd

## 🧪 Testing

### Run All Tests
dotnet test
### Run with Coverage
dotnet test --collect:"XPlat Code Coverage"

### Test Categories

- **Unit Tests**: Domain entities, value objects, handlers, and services
- **Integration Tests**: Full API testing with test server
- **Infrastructure Tests**: Repository and external service integrations

### Test Structure
WeatherService.Tests/
├── Domain/
│   ├── Entities/
│   └── ValueObjects/
├── Application/
│   └── Handlers/
├── Infrastructure/
│   └── Services/
├── Controllers/
└── Integration/

## 🏛️ Clean Architecture Patterns

### Domain-Driven Design (DDD)
- **Entities**: `WeatherData` with business rules and validation
- **Value Objects**: `Coordinates` with immutability and equality
- **Domain Services**: Business logic that doesn't fit in entities

### CQRS (Command Query Responsibility Segregation)
- **Queries**: `GetWeatherByCoordinatesQuery`, `GetWeatherByCityQuery`
- **Handlers**: Separate handlers for each query type
- **MediatR**: Mediator pattern for loose coupling

### Repository Pattern
- **Interfaces**: Domain-defined repository contracts
- **Implementations**: Infrastructure-layer MongoDB implementation
- **Dependency Inversion**: Business logic depends on abstractions

### Dependency Injection
- **Service Registration**: All dependencies registered in `Program.cs`
- **Interface Segregation**: Focused, single-purpose interfaces
- **Testability**: Easy mocking and testing

## 📊 Caching Strategy

The application implements intelligent caching:

1. **First Call**: Data fetched from Open-Meteo API and cached in MongoDB
2. **Subsequent Calls**: Data served from cache if not expired
3. **Cache Expiration**: 1 hour TTL with automatic cleanup
4. **Cache Keys**: Based on coordinates or city name

### Cache Benefits
- Reduced external API calls
- Improved response times
- Cost optimization
- Offline resilience

## 🔍 Monitoring and Observability

### Logging
- **Structured Logging**: JSON-formatted logs
- **Log Levels**: Information, Warning, Error
- **Request Tracking**: Correlation IDs for request tracing

### Health Checks
- **Endpoint**: `/health`
- **MongoDB**: Connection health
- **External APIs**: Service availability

### Metrics
- API response times
- Cache hit/miss ratios
- External API call frequency

## 🔒 Security Considerations

### Input Validation
- Coordinate range validation (-90 to 90 for latitude, -180 to 180 for longitude)
- City name sanitization
- Model validation attributes

### Error Handling
- No sensitive data in error responses
- Graceful degradation on external service failures
- Proper HTTP status codes

### Rate Limiting
- Consider implementing rate limiting for production
- MongoDB TTL indexes for automatic cleanup

## 🚀 Deployment

### Docker (Optional)

Create a `Dockerfile`:
ROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base WORKDIR /app EXPOSE 80 EXPOSE 443
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build WORKDIR /src COPY ["WeatherService/WeatherService.csproj", "WeatherService/"] RUN dotnet restore "WeatherService/WeatherService.csproj" COPY . . WORKDIR "/src/WeatherService" RUN dotnet build "WeatherService.csproj" -c Release -o /app/build
FROM build AS publish RUN dotnet publish "WeatherService.csproj" -c Release -o /app/publish
FROM base AS final WORKDIR /app COPY --from=publish /app/publish . ENTRYPOINT ["dotnet", "WeatherService.dll"]

### Production Checklist
- [ ] Configure HTTPS certificates
- [ ] Set up MongoDB replica set
- [ ] Configure logging aggregation
- [ ] Set up monitoring and alerting
- [ ] Implement rate limiting
- [ ] Configure CORS policies
- [ ] Set up CI/CD pipeline

## 📚 Learning Objectives

This project demonstrates:

### Clean Architecture Principles
- **Dependency Inversion**: High-level modules don't depend on low-level modules
- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Interface Segregation**: Clients depend on interfaces they use

### Modern .NET Patterns
- **CQRS**: Separate read and write operations
- **MediatR**: Mediator pattern implementation
- **Dependency Injection**: Built-in DI container usage
- **Async/Await**: Proper asynchronous programming

### Testing Best Practices
- **Unit Testing**: Isolated component testing
- **Integration Testing**: End-to-end API testing
- **Mocking**: Using Moq for test doubles
- **Test Structure**: Arrange-Act-Assert pattern

### Production Readiness
- **Error Handling**: Comprehensive exception management
- **Logging**: Structured logging with context
- **Validation**: Input validation and sanitization
- **Documentation**: OpenAPI/Swagger documentation

## 🤝 Contributing

This is an educational project. Feel free to:

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Submit a pull request

### Code Standards
- Follow Clean Architecture principles
- Maintain test coverage above 80%
- Use descriptive naming conventions
- Add XML documentation for public APIs

## 📖 Additional Resources

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- [Domain-Driven Design](https://martinfowler.com/tags/domain%20driven%20design.html)
- [.NET 6 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [MongoDB .NET Driver](https://docs.mongodb.com/drivers/csharp/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)

## 📄 License

This project is for educational purposes. Use it as a reference for learning Clean Architecture and modern .NET development practices.

---
**Built with ❤️ by Leonardo Prado**
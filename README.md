# Clean Architecture Template

A modern .NET 10 template implementing Clean Architecture principles with comprehensive layering, domain-driven design, and integration testing.

## 🏗️ Project Structure

```
clean-arch-template/
├── src/
│   ├── Api/                    # Presentation Layer (ASP.NET Core API)
│   ├── Application/            # Application Layer (Use Cases & DTOs)
│   ├── Infrastructure/         # Infrastructure Layer (Data Access, Services)
│   └── Domain/                 # Domain Layer (Entities, Interfaces)
├── tests/
│   ├── DomainUnitTest/        # Domain layer unit tests
│   ├── ApplicationUnitTests/   # Application layer unit tests
│   ├── ApiIntegrationTests/    # API integration tests
│   └── InfrastructureIntegrationTests/  # Infrastructure integration tests
└── README.md
```

## 🎯 Layers

- **Domain**: Core business logic and entities
- **Application**: Business rules and use cases
- **Infrastructure**: External services and data persistence
- **Api**: REST endpoints and controllers

## 🔧 Getting Started

### Prerequisites
- .NET 10 SDK or later
- Visual Studio 2022+ or VS Code

### Building the Project
```bash
dotnet build
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/DomainUnitTest/DomainUnitTests.csproj
```

### Running the API
```bash
dotnet run --project src/Api
```

## ✨ Features

- Clean Architecture principles
- Domain-Driven Design
- Separation of concerns
- Comprehensive unit and integration tests
- .NET 10 target framework
- RESTful API design

## 📝 License

See LICENSE.txt for details.
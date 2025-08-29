# Fanex.Affiliate.Link - .NET Core 9 Migration

This project has been migrated from .NET Framework 4.7.2 to .NET Core 9.

## Migration Changes

### Project Structure Changes
- Converted from old-style `.csproj` to SDK-style project file
- Replaced `Global.asax.cs` with `Program.cs`
- Replaced `Web.config` with `appsettings.json`
- Removed `packages.config` in favor of PackageReference
- Updated Views structure for .NET Core

### Key Updates
- Updated all controllers to use dependency injection
- Converted `System.Web.Mvc` to `Microsoft.AspNetCore.Mvc`
- Updated HTTP request handling for .NET Core
- Migrated configuration from `ConfigurationManager` to `IConfiguration`
- Updated logging configuration for .NET Core

### Dependencies
- Target Framework: .NET 9.0
- ASP.NET Core MVC
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging

## Running the Application

### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or later (recommended)

### Development
1. Clone the repository
2. Navigate to the project directory
3. Run `dotnet restore` to restore packages
4. Run `dotnet build` to build the project
5. Run `dotnet run` to start the application

### Configuration
- Update `appsettings.json` with your environment-specific settings
- Connection strings and other configuration are now in JSON format
- Environment-specific settings can be added in `appsettings.{Environment}.json`

## Deployment
- The application can be deployed to any platform that supports .NET Core
- Use `dotnet publish` to create deployment packages
- Configure environment variables for production settings

## Notes
- All existing functionality has been preserved
- Routes and controllers maintain the same behavior
- Static files should be placed in the `wwwroot` folder
- The application uses the new .NET Core middleware pipeline 
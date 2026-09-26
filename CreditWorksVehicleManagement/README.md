# CreditWorks Vehicle Management

A web application for managing vehicles and vehicle weight categories, developed as part of the CreditWorks Software Engineer technical assignment.

The application allows users to:

- Add and view vehicles
- Select a manufacturer from the configured manufacturer list
- Automatically determine a vehicle's category from its weight
- Sort vehicles by owner, manufacturer, year, and weight
- Create, edit, and delete vehicle weight categories
- Assign an icon to each category
- Maintain continuous category ranges with no gaps or overlaps
- Automatically reflect category definition changes for existing vehicles

## Technology Stack

- C#
- ASP.NET Core MVC
- .NET 9
- Entity Framework Core 9
- SQL Server
- SQL Server LocalDB for local development
- Razor Views
- Bootstrap
- xUnit
- Entity Framework Core InMemory provider for unit tests

## Project Structure

The solution contains two projects:

### CreditWorksVehicleManagement

The main ASP.NET Core MVC application.

Main areas:

- `Controllers` - Handles HTTP requests and coordinates application operations
- `Models` - Domain entities
- `ViewModels` - Models used specifically by the UI
- `Services` - Business logic, including vehicle category rules
- `Data` - EF Core DbContext and database configuration
- `Views` - Razor UI views
- `Migrations` - EF Core database migrations
- `wwwroot` - Static CSS and other frontend assets

### CreditWorksVehicleManagement.Tests

Contains automated tests for:

- Category determination
- Category boundary behaviour
- Category configuration validation
- Category changes
- Vehicle validation
- Vehicle sorting

## Database

The application uses SQL Server through Entity Framework Core.

For local development, the configured SQL Server instance is:

```text
(localdb)\MSSQLLocalDB
# secure-CRUD

`secure-CRUD` is a .NET 8 full-stack academic project that demonstrates a secure CRUD-style application with a browser dashboard, ASP.NET Core service layer, business layer, data-access layer, and MySQL database.

## What This Project Does

The project provides:

- a browser-based dashboard for device and user operations
- an ASP.NET Core minimal API service host
- business-layer services for validation and application rules
- a MySQL-backed data layer for CRUD and query operations
- an optional console client that calls the same API as the browser client

Main supported functionality includes:

- create device
- update device
- get all devices
- get a device by ID
- get totals for devices and users
- database explorer queries for all rows, one row by key, and filtered subsets
- optional delete device

## Why This Project Is Useful

This repository is useful as a reference for:

- layered application design in .NET
- hosting a front end and API from one ASP.NET Core project
- connecting a service and DAL to MySQL
- demonstrating full-system CRUD testing with database proof
- academic documentation for deployment and verification

## Architecture

Projects in the solution:

- `src/EdgeAdmin.Service` - ASP.NET Core service host and browser front end
- `src/EdgeAdmin.Business` - business logic and DTOs
- `src/EdgeAdmin.DAL` - MySQL repositories
- `src/EdgeAdmin.Shared` - shared models
- `src/EdgeAdmin.ConsoleUI` - optional console client

Runtime flow:

`Browser client -> ASP.NET Core service -> business services -> DAL repositories -> MySQL`

## Getting Started

### Prerequisites

- Windows with PowerShell
- .NET SDK 8.x
- MySQL Server 8.x
- MySQL Workbench or another MySQL client
- Visual Studio 2022, VS Code, or another IDE with .NET support

### Quick Start

1. Download the repository ZIP from GitHub and extract it.
2. Open the extracted folder in PowerShell or your IDE.
3. Create or verify the MySQL database named `addmin`.
4. Configure the service connection string in `src/EdgeAdmin.Service/appsettings.Development.json` or with `DB_CONNECTION_STRING`.
5. Verify MySQL is running and reachable on `127.0.0.1:3306`.
6. Build and run the service:

```powershell
dotnet build .\secure-CRUD.sln
dotnet run --project .\src\EdgeAdmin.Service\EdgeAdmin.Service.csproj
```

7. Open the local URL shown by ASP.NET Core in a browser.
8. Optional: open a second terminal and run the console client:

```powershell
dotnet run --project .\src\EdgeAdmin.ConsoleUI\EdgeAdmin.ConsoleUI.csproj
```

The browser dashboard is hosted by the ASP.NET Core service. The console app is a second client, not a separate host.

## Documentation

Detailed project documentation is available in:

- [Deployment guide](docs/DEPLOYMENT.md)
- [Full-system test guide](docs/FULL-SYSTEM-TEST.md)
- [Screenshot checklist](docs/screenshots/README.md)
- [SQL helper queries](sql/queries/count_rows.sql)

## Getting Help

If the app starts but data does not load:

- confirm the MySQL service is running
- confirm port `3306` is reachable
- confirm the connection string points to the correct MySQL instance
- confirm the `addmin` database and required tables exist

The detailed database connectivity recovery flow is in [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md).

## Maintenance And Contributions

This repository is maintained by the repository owner and project contributors.

Contributions to documentation, testing evidence, and project improvements should follow the existing project structure and keep the deployment and verification steps accurate.

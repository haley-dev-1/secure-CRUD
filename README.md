# secure-CRUD

Secure CRUD is a .NET 8 full-stack academic project with these layers:

- Client: browser dashboard in `src/EdgeAdmin.Service/wwwroot`
- Service host: ASP.NET Core minimal API in `src/EdgeAdmin.Service`
- Business layer: `src/EdgeAdmin.Business`
- Data layer: `src/EdgeAdmin.DAL`
- Shared models: `src/EdgeAdmin.Shared`
- Optional second client: console UI in `src/EdgeAdmin.ConsoleUI`

The browser client is hosted by the same ASP.NET Core service that exposes the API, so the deployed system is:

`Browser -> ASP.NET Core service -> business services -> repositories -> MySQL`

## Repository Quick Start

Prerequisites:

- Windows with PowerShell
- .NET SDK 8.x
- MySQL Server 8.x
- An IDE such as Visual Studio 2022 or VS Code with the C# extension

1. Download the repository ZIP from GitHub and extract it.
2. Open the extracted folder.
3. Create a MySQL database named `addmin`.
4. Set the connection string in `src/EdgeAdmin.Service/appsettings.Development.json` or with the `DB_CONNECTION_STRING` environment variable.
5. Build the solution:

```powershell
dotnet build .\secure-CRUD.sln
```

6. Run the service host:

```powershell
dotnet run --project .\src\EdgeAdmin.Service\EdgeAdmin.Service.csproj
```

7. Open the local URL printed by ASP.NET Core in a browser.
8. Use the dashboard to test create, update, get-all, get-by-id, and database explorer flows.

## What Works

The service currently exposes these main routes:

- `GET /api/devices`
- `GET /api/devices/{id}`
- `POST /api/devices`
- `PUT /api/devices/by-public-id/{publicDeviceId}`
- `DELETE /api/devices/{id}`
- `GET /api/devices/total`
- `GET /api/users/total`
- `GET /api/users/{userId}/last-device`
- `GET /api/users/{userId}/dates`
- `POST /api/users/{userId}/mark-inactive-if-stale`
- `GET /api/db/tables`
- `GET /api/db/tables/{tableName}/rows`
- `GET /api/db/tables/{tableName}/rows/by-key/{keyValue}`
- `GET /api/db/tables/{tableName}/rows/by-column/{columnName}?value=...`

## Deliverable Docs

- Deployment guide: [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md)
- Full-system test and screenshot guide: [docs/FULL-SYSTEM-TEST.md](docs/FULL-SYSTEM-TEST.md)
- Screenshot checklist: [docs/screenshots/README.md](docs/screenshots/README.md)

## Optional Console Client

If you also want to test the API from the console client, set `ServiceBaseUrl` in its appsettings and run:

```powershell
dotnet run --project .\src\EdgeAdmin.ConsoleUI\EdgeAdmin.ConsoleUI.csproj
```

## Database Screenshot Helpers

Helper SQL for screenshot evidence is in:

- [sql/queries/count_rows.sql](sql/queries/count_rows.sql)

## Verification

Your setup succeeded when:

- `dotnet build` completes successfully
- the service starts without a connection-string error
- the browser dashboard loads
- dashboard actions return HTTP 200 in the response inspector
- database queries confirm the same inserts and updates shown in the UI

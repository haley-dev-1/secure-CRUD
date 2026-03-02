# secure-CRUD
Academic project - can chatGPT/LLM tools make a secure full stack application?

## Browser UI + Service
Prereqs:
- .NET SDK 8.x
- MySQL Server 8.x running locally

Setup:
1. Create or verify the database `addmin`.
2. Configure the service connection string locally in one of these ways:
   - `src/EdgeAdmin.Service/appsettings.Development.json`
   - `DB_CONNECTION_STRING` environment variable
3. Configure the console client `ServiceBaseUrl` only if you still plan to use the console UI.

Run:
```powershell
dotnet build .\secure-CRUD.sln
dotnet run --project .\src\EdgeAdmin.Service\EdgeAdmin.Service.csproj
```

Open the local service URL in a browser to use the front end. The dashboard calls the same `/api/...` routes used by the console client.
It also includes a database explorer that discovers all tables from the active schema and can perform:
- get all rows
- get a single row by primary key
- get a filtered subset by column value

Current dashboard areas:
- device inventory with internal scrolling
- tabbed create-device flow
- compact update/delete cards
- user lookup actions
- response inspector
- database explorer for all discovered tables

## Console UI (PowerShell)
- The original console client still exists in `src/EdgeAdmin.ConsoleUI`.
- If you want to keep using it, set `ServiceBaseUrl` in its local appsettings and run:

```powershell
dotnet run --project .\src\EdgeAdmin.ConsoleUI\EdgeAdmin.ConsoleUI.csproj
```

## Service Layer Host
- Platform: ASP.NET Core Web API (minimal API) in `src/EdgeAdmin.Service`.
- All business-layer methods are exposed as HTTP endpoints under `/api/...`.
- See `src/EdgeAdmin.Service/Program.cs` comments for hosting notes (local Kestrel, Azure App Service, IIS, Docker).

## Suggested Console Test Flow (for screenshots)
1. `8` Get device by id (baseline check).
2. `5` Create device.
3. `8` Get device by id using returned id.
4. `6` Update device.
5. `8` Get device by id to verify update.
6. `7` Delete device.
7. `8` Get device by id to verify deletion result.

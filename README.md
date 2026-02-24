# secure-CRUD
Academic project - can chatGPT/LLM tools make a secure full stack application?

## Console UI (PowerShell)
Prereqs:
- .NET SDK 8.x
- MySQL Server 8.x running locally

Setup:
1. Create or verify the database `addmin` and tables from `sql/schema`.
2. Configure the connection string in `src/EdgeAdmin.ConsoleUI/appsettings.Development.json`.
   - This file is ignored by git; set the password locally.

Run:
```powershell
dotnet build .\secure-CRUD.sln
dotnet run --project .\src\EdgeAdmin.Service\EdgeAdmin.Service.csproj
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

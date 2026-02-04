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
dotnet run --project .\src\EdgeAdmin.ConsoleUI\EdgeAdmin.ConsoleUI.csproj
```

# Deployment Guide

## 1. Purpose

This document explains how to go from a GitHub repository ZIP download to a working secure-CRUD deployment with:

- MySQL as the database platform
- ASP.NET Core on .NET 8 as the service host
- the browser dashboard hosted by the ASP.NET Core service

In this project, the business layer and data layer are not deployed separately. They are compiled into the service host and accessed through HTTP endpoints exposed by `src/EdgeAdmin.Service`.

## 2. Architecture

Runtime flow:

`Browser client -> ASP.NET Core service -> business services -> DAL repositories -> MySQL`

Projects:

- `src/EdgeAdmin.Service`: service host and static front end
- `src/EdgeAdmin.Business`: business rules and DTOs
- `src/EdgeAdmin.DAL`: MySQL data access
- `src/EdgeAdmin.Shared`: shared models
- `src/EdgeAdmin.ConsoleUI`: optional API client for additional testing

## 3. Prerequisites

Install these before building:

- .NET SDK 8.x
- MySQL Server 8.x
- GitHub access or a downloaded ZIP of the repository
- Visual Studio 2022, VS Code, or another IDE that supports .NET 8
- PowerShell

Recommended settings:

- MySQL host: `127.0.0.1`
- MySQL port: `3306`
- Database name: `addmin`
- Service environment: Development for local deployment

## 4. Download And Open The Project

1. Open the GitHub repository page.
2. Select `Code`.
3. Select `Download ZIP`.
4. Extract the ZIP to a local folder.
5. Open PowerShell in the extracted project root.
6. Confirm the solution file exists:

```powershell
dir .\secure-CRUD.sln
```

## 5. Configure The Database

1. Start MySQL Server.
2. Connect using MySQL Workbench, the MySQL CLI, or another SQL tool.
3. Create the database if it does not already exist:

```sql
CREATE DATABASE IF NOT EXISTS addmin;
USE addmin;
```

4. Ensure the schema used by the code exists.

The code expects at least these tables and columns:

- `devices`
  - `device_id`
  - `device_uid`
  - `device_type_id`
  - `owner_user_id`
  - `nickname`
  - `created_at`
- `user_accounts`
  - `user_id`
  - `created_at`
  - `updated_at`
  - `status`

5. Load your schema and seed data into `addmin`.

The repository currently includes SQL helper documentation and screenshot query helpers, but the actual schema and seed scripts are not present in this repo snapshot. If you maintain them elsewhere, import them now before starting the service.

6. Verify that data exists:

```sql
USE addmin;
SELECT COUNT(*) AS device_count FROM devices;
SELECT COUNT(*) AS user_count FROM user_accounts;
```

If your assignment requires 50 or more rows of seed data, capture a screenshot of those counts after import.

## 6. Configure The Connection String

The service reads the database connection from:

- `ConnectionStrings:Db` in `src/EdgeAdmin.Service/appsettings.Development.json`
- or `DB_CONNECTION_STRING` from the environment

Default local file:

```json
{
  "ConnectionStrings": {
    "Db": "server=127.0.0.1;port=3306;database=addmin;user=root;password=REPLACE_ME;"
  }
}
```

Update `REPLACE_ME` to the correct password, or set an environment variable instead:

```powershell
$env:DB_CONNECTION_STRING="server=127.0.0.1;port=3306;database=addmin;user=root;password=your_password;"
```

## 7. Build The Solution

Run:

```powershell
dotnet build .\secure-CRUD.sln
```

Build succeeded if:

- the command exits without errors
- all projects compile
- no missing package or SDK errors are reported

## 8. Run And Host The Back End

This project uses ASP.NET Core minimal APIs as the service platform.

Local hosting command:

```powershell
dotnet run --project .\src\EdgeAdmin.Service\EdgeAdmin.Service.csproj
```

What this hosts:

- service endpoints under `/api/...`
- the browser client from `wwwroot`

The service starts correctly when:

- ASP.NET Core prints local listening URLs
- no `Missing connection string` exception appears
- no MySQL connection failure appears on startup or first request

## 9. Host The Front End

The front end is the static dashboard in:

- `src/EdgeAdmin.Service/wwwroot/index.html`
- `src/EdgeAdmin.Service/wwwroot/app.js`
- `src/EdgeAdmin.Service/wwwroot/styles.css`

You do not need a separate frontend hosting platform for local deployment. The ASP.NET Core service already hosts the client through:

- `app.UseDefaultFiles();`
- `app.UseStaticFiles();`
- `app.MapFallbackToFile("index.html");`

That makes the most appropriate client-hosting choice for this project the same ASP.NET Core host as the API.

## 10. Optional Console Client

The console app is an optional second client that calls the same service endpoints.

It reads `ServiceBaseUrl` and defaults to:

```text
http://localhost:5000
```

Run it with:

```powershell
dotnet run --project .\src\EdgeAdmin.ConsoleUI\EdgeAdmin.ConsoleUI.csproj
```

Use it if you want additional proof that multiple clients can consume the same service layer.

## 11. Functional Verification

After the service starts, open the browser URL shown in the terminal.

The deployment is successful if the following work:

1. The dashboard loads without a blank page or script error.
2. `Load total devices` returns a value.
3. `Load total users` returns a value.
4. `List all devices` fills the inventory table.
5. `Create device` returns HTTP 200 and the new record appears in the device list.
6. `Get device by ID` returns the created record.
7. `Update device` returns HTTP 200 and the changed values appear in the device list.
8. `Database Explorer` can:
   - list tables
   - return all rows for a selected table
   - return one row by primary key
   - return a filtered subset by column value
9. Optional: `Delete device` removes the record and follow-up lookup shows it is gone.

## 12. Suggested Manual Test Order

Use this order to produce clean evidence:

1. Capture the dashboard landing page.
2. Capture the database row counts.
3. Create a device.
4. Query the inserted row in MySQL.
5. Get that device by ID from the dashboard.
6. Update the same device.
7. Query the updated row in MySQL.
8. Use the database explorer to show all rows.
9. Use the database explorer to get a single row by key.
10. Optional: delete the device and confirm it no longer appears.

## 13. Files To Include As Evidence

Suggested screenshot set:

- dashboard overview
- successful create action in the client
- inserted database row
- successful update action in the client
- updated database row
- get-all result in the client
- get-all SQL result
- get-single result in the client
- get-single SQL result
- optional delete result in the client
- optional delete verification query result

## 14. Troubleshooting

If the dashboard loads but requests fail:

- confirm the service is still running
- confirm the database connection string is correct
- confirm MySQL is running on the host and port in the connection string
- confirm the `addmin` schema exists
- confirm required tables exist

If create fails:

- confirm `owner_user_id` references an existing user
- confirm the submitted `device_type_id` exists in your schema if that column is constrained

If user operations fail:

- confirm the selected `user_id` exists in `user_accounts`

If database explorer fails:

- confirm the logged-in database user has permission to read `information_schema`

## 15. Completion Check

From a fresh ZIP download, setup is complete when another user can:

1. install prerequisites
2. configure the MySQL connection
3. build the solution
4. run the service
5. open the dashboard
6. create, update, and retrieve records successfully
7. verify those changes directly in the database

# Full System Test Guide

## Goal

This guide produces evidence that the full system works across all layers:

- client layer: browser dashboard
- service layer: ASP.NET Core API
- business layer: validation and service methods
- data layer: MySQL repositories

The required functionality for this project is:

- insert
- update
- get all
- get one by unique value or ID

Delete is optional, but this project supports it and it is included below.

## Test Preconditions

Before testing:

1. Start MySQL and confirm the `addmin` database is available.
2. Confirm the service connection string is valid.
3. Run the service:

```powershell
dotnet run --project .\src\EdgeAdmin.Service\EdgeAdmin.Service.csproj
```

4. Open the dashboard in a browser.
5. Open your SQL client against the same `addmin` database.

## Evidence Rules

For each test, capture:

- one screenshot from the client layer
- one screenshot from the database layer

When possible, include the browser response inspector and the SQL result grid in the same screenshot set.

## Recommended Screenshot File Names

- `dashboard-overview.png`
- `seed_count.png`
- `create-device-client.png`
- `create-device-db.png`
- `get-device-client.png`
- `get-device-db.png`
- `update-device-client.png`
- `update-device-db.png`
- `get-all-client.png`
- `get-all-db.png`
- `get-one-by-key-client.png`
- `get-one-by-key-db.png`
- `delete-device-client.png`
- `delete-device-db.png`

## Test Data

Use a unique public device ID so your screenshots are unambiguous.

Example values:

- original public device ID: `dev_stage3_001`
- updated public device ID: `dev_stage3_001_updated`
- original nickname/name: `Stage 3 Test Device`
- updated nickname/name: `Stage 3 Updated Device`

Use a real `owner_user_id` that already exists in `user_accounts`.

## SQL Helpers

You can use the helper file:

- [sql/queries/count_rows.sql](../sql/queries/count_rows.sql)

Additional useful queries:

```sql
USE addmin;

SELECT device_id, device_uid, owner_user_id, nickname, created_at
FROM devices
WHERE device_uid IN ('dev_stage3_001', 'dev_stage3_001_updated')
ORDER BY device_id DESC;
```

```sql
USE addmin;

SELECT *
FROM devices
ORDER BY device_id DESC;
```

## Test 1: Baseline Proof

Client evidence:

- capture the dashboard landing page
- show the inventory area and response inspector

Database evidence:

- run row-count queries for `devices` and `user_accounts`
- capture the result grid

Success criteria:

- dashboard loads
- row counts return successfully

## Test 2: Insert A New Device

Client steps:

1. In `Create device`, enter a unique public device ID.
2. Enter a device name.
3. Select a device type ID.
4. Enter an existing owner user ID.
5. Submit the form.
6. Capture the browser showing HTTP 200 in the response inspector.

Database steps:

```sql
USE addmin;

SELECT device_id, device_uid, owner_user_id, nickname, created_at
FROM devices
WHERE device_uid = 'dev_stage3_001';
```

Capture the query result showing the inserted row.

Success criteria:

- service returns success
- the inserted row exists in MySQL

## Test 3: Get The Inserted Device By ID

Client steps:

1. Copy the `device_id` returned by the create operation.
2. Use `Get device by ID`.
3. Capture the browser result.

Database steps:

```sql
USE addmin;

SELECT device_id, device_uid, owner_user_id, nickname, created_at
FROM devices
WHERE device_id = <inserted_id>;
```

Capture the matching row.

Success criteria:

- the browser returns the same record as the database

## Test 4: Update The Device

Client steps:

1. In `Update device`, set:
   - target public device ID = `dev_stage3_001`
   - new public device ID = `dev_stage3_001_updated`
   - name = `Stage 3 Updated Device`
2. Submit the form.
3. Capture the successful response in the browser.

Database steps:

```sql
USE addmin;

SELECT device_id, device_uid, owner_user_id, nickname, created_at
FROM devices
WHERE device_uid = 'dev_stage3_001_updated';
```

Capture the updated row.

Success criteria:

- the old public ID no longer represents the active record
- the new public ID and updated name are stored in MySQL

## Test 5: Get All Devices

Client steps:

1. Select `List all devices`.
2. Capture the inventory table showing the updated device.

Database steps:

```sql
USE addmin;

SELECT device_id, device_uid, owner_user_id, nickname, created_at
FROM devices
ORDER BY device_id DESC;
```

Capture the result set.

Success criteria:

- the client list matches the database contents

## Test 6: Get One Record By Unique Value Or Key

This can be shown with the database explorer.

Client steps:

1. Open `Database Explorer`.
2. Select the `devices` table.
3. Use `Get single record` with the inserted `device_id`.
4. Capture the returned row.

Database steps:

```sql
USE addmin;

SELECT *
FROM devices
WHERE device_id = <inserted_id>;
```

Capture the same row in SQL.

Success criteria:

- both client and database show one matching record

## Test 7: Optional Delete

Client steps:

1. Use `Delete device` with the inserted `device_id`.
2. Capture the successful response.

Database steps:

```sql
USE addmin;

SELECT *
FROM devices
WHERE device_id = <inserted_id>;
```

Capture the empty result.

Success criteria:

- the row is no longer present

## Final Submission Checklist

Include:

- deployment document
- updated README
- screenshots proving create, update, get-all, get-one, and optional delete
- screenshots proving the same results directly in MySQL

The strongest final evidence bundle is a chronological set of screenshots that shows one record being created, read, updated, optionally deleted, and checked in both the UI and the database after each step.

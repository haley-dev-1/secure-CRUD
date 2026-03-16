USE addmin;

SELECT 'devices' AS table_name, COUNT(*) AS row_count
FROM devices
UNION ALL
SELECT 'user_accounts' AS table_name, COUNT(*) AS row_count
FROM user_accounts;

SELECT device_id, device_uid, owner_user_id, nickname, created_at
FROM devices
ORDER BY device_id DESC
LIMIT 25;

SELECT user_id, created_at, updated_at, status
FROM user_accounts
ORDER BY user_id DESC
LIMIT 25;

using EdgeAdmin.DAL.Abstractions;
using EdgeAdmin.DAL.Db;
using EdgeAdmin.Shared.Models;
using MySqlConnector;

namespace EdgeAdmin.DAL.Repositories;

public sealed class DeviceRepository : ICrudRepository<Device, long>, IDeviceQueryRepository
{
    private readonly MySqlConnectionFactory _factory;

    public DeviceRepository(MySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = @"
                    SELECT device_id,
                           device_uid AS device_guid,
                           nickname AS display_name,
                           created_at AS created_at_utc
                    FROM devices
                    ORDER BY device_id DESC;
                    ";

        var list = new List<Device>();

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);

        while (await r.ReadAsync(ct))
            list.Add(Map(r));

        return list;
    }

    public async Task<Device?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        const string sql = @"
                SELECT device_id,
                       device_uid AS device_guid,
                       nickname AS display_name,
                       created_at AS created_at_utc
                FROM devices
                WHERE device_id = @id
                LIMIT 1;
                ";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;

        return Map(r);
    }

    public async Task<long> CreateAsync(Device entity, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO devices (device_uid, device_type_id, owner_user_id, nickname, created_at)
            VALUES (@guid, @deviceTypeId, @ownerUserId, @name, @created);

            SELECT LAST_INSERT_ID();
            ";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@guid", entity.DeviceGuid);
        cmd.Parameters.AddWithValue("@deviceTypeId", entity.DeviceTypeId);
        cmd.Parameters.AddWithValue("@ownerUserId", entity.OwnerUserId);
        cmd.Parameters.AddWithValue("@name", entity.DisplayName);
        cmd.Parameters.AddWithValue("@created", entity.CreatedAtUtc);

        var scalar = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(scalar);
    }

    public async Task<bool> UpdateAsync(Device entity, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE devices
            SET device_uid = @guid,
                nickname = @name
            WHERE device_id = @id;
            ";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", entity.DeviceId);
        cmd.Parameters.AddWithValue("@guid", entity.DeviceGuid);
        cmd.Parameters.AddWithValue("@name", entity.DisplayName);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> UpdateByPublicDeviceIdAsync(
        string targetPublicDeviceId,
        string newPublicDeviceId,
        string name,
        CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE devices
            SET device_uid = @newGuid,
                nickname = @name
            WHERE device_uid = @targetGuid;
            ";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@targetGuid", targetPublicDeviceId);
        cmd.Parameters.AddWithValue("@newGuid", newPublicDeviceId);
        cmd.Parameters.AddWithValue("@name", name);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct = default)
    {
        const string sql = @"DELETE FROM devices WHERE device_id = @id;";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<int> GetTotalDevicesAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT COUNT(*) FROM devices;";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        var scalar = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(scalar);
    }

    public async Task<int> GetTotalUsersAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT COUNT(*) FROM user_accounts;";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        var scalar = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(scalar);
    }

    public async Task<Device?> GetMostRecentDeviceByUserIdAsync(long userId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT device_id,
                   device_uid AS device_guid,
                   nickname AS display_name,
                   created_at AS created_at_utc
            FROM devices
            WHERE owner_user_id = @userId
            ORDER BY created_at DESC
            LIMIT 1;
            ";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;

        return Map(r);
    }

    private static Device Map(MySqlDataReader r)
    {
        return new Device
        {
            DeviceId = r.GetInt64("device_id"),
            DeviceGuid = r.GetString("device_guid"),
            DisplayName = r.GetString("display_name"),
            CreatedAtUtc = r.GetDateTime("created_at_utc")
        };
    }
}

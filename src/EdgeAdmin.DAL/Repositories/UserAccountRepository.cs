using EdgeAdmin.DAL.Abstractions;
using EdgeAdmin.DAL.Db;
using EdgeAdmin.Shared.Models;
using MySqlConnector;
using System.Text.RegularExpressions;

namespace EdgeAdmin.DAL.Repositories;

public sealed class UserAccountRepository : IUserAccountRepository
{
    private readonly MySqlConnectionFactory _factory;

    public UserAccountRepository(MySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<bool> ExistsAsync(long userId, CancellationToken ct = default)
    {
        const string sql = "SELECT 1 FROM user_accounts WHERE user_id = @userId LIMIT 1;";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        var scalar = await cmd.ExecuteScalarAsync(ct);
        return scalar is not null;
    }

    public async Task<UserAccount?> GetByIdAsync(long userId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT user_id,
                   created_at,
                   updated_at,
                   status
            FROM user_accounts
            WHERE user_id = @userId
            LIMIT 1;
            ";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct)) return null;

        return new UserAccount
        {
            UserId = r.GetInt64("user_id"),
            CreatedAtUtc = r.GetDateTime("created_at"),
            UpdatedAtUtc = r.GetDateTime("updated_at"),
            Status = r.GetString("status")
        };
    }

    public async Task<bool> UpdateStatusAsync(long userId, string status, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE user_accounts
            SET status = @status,
                updated_at = NOW()
            WHERE user_id = @userId;
            ";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@userId", userId);

        var rows = await cmd.ExecuteNonQueryAsync(ct);
        return rows > 0;
    }

    public async Task<string> ResolveInactiveStatusValueAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT data_type,
                   column_type,
                   character_maximum_length
            FROM information_schema.columns
            WHERE table_schema = DATABASE()
              AND table_name = 'user_accounts'
              AND column_name = 'status'
            LIMIT 1;
            ";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        await using var r = await cmd.ExecuteReaderAsync(ct);

        if (!await r.ReadAsync(ct))
        {
            return "inactive";
        }

        var dataType = r.GetString("data_type");
        var columnType = r.GetString("column_type");
        var maxLen = r.IsDBNull(r.GetOrdinal("character_maximum_length"))
            ? (long?)null
            : r.GetInt64("character_maximum_length");

        if (string.Equals(dataType, "enum", StringComparison.OrdinalIgnoreCase))
        {
            var allowed = ParseEnumValues(columnType);
            var match = allowed.FirstOrDefault(x =>
                           x.Contains("inactive", StringComparison.OrdinalIgnoreCase))
                        ?? allowed.FirstOrDefault(x =>
                           x.Contains("disabled", StringComparison.OrdinalIgnoreCase))
                        ?? allowed.FirstOrDefault(x =>
                           x.Contains("suspend", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(match))
            {
                return match;
            }

            throw new InvalidOperationException(
                "Could not find an inactive-like enum value in user_accounts.status.");
        }

        const string target = "inactive";
        if (maxLen is > 0 && target.Length > maxLen)
        {
            return target[..(int)maxLen.Value];
        }

        return target;
    }

    private static IReadOnlyList<string> ParseEnumValues(string columnType)
    {
        var matches = Regex.Matches(columnType, @"'([^']*)'");
        return matches.Select(m => m.Groups[1].Value).ToList();
    }
}

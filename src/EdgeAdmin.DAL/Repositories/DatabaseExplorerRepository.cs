using EdgeAdmin.DAL.Abstractions;
using EdgeAdmin.DAL.Db;
using MySqlConnector;

namespace EdgeAdmin.DAL.Repositories;

public sealed class DatabaseExplorerRepository : IDatabaseExplorerRepository
{
    private readonly MySqlConnectionFactory _factory;

    public DatabaseExplorerRepository(MySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<DatabaseTableInfo>> GetTablesAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT c.table_name,
                   c.column_name,
                   c.data_type,
                   c.is_nullable,
                   c.column_key,
                   c.ordinal_position
            FROM information_schema.columns c
            WHERE c.table_schema = DATABASE()
            ORDER BY c.table_name, c.ordinal_position;
            ";

        var tableMap = new Dictionary<string, List<DatabaseColumnInfo>>(StringComparer.OrdinalIgnoreCase);
        var primaryKeys = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var tableName = reader.GetString("table_name");
            var column = new DatabaseColumnInfo
            {
                Name = reader.GetString("column_name"),
                DataType = reader.GetString("data_type"),
                IsNullable = string.Equals(reader.GetString("is_nullable"), "YES", StringComparison.OrdinalIgnoreCase),
                IsPrimaryKey = string.Equals(reader.GetString("column_key"), "PRI", StringComparison.OrdinalIgnoreCase)
            };

            if (!tableMap.TryGetValue(tableName, out var columns))
            {
                columns = [];
                tableMap[tableName] = columns;
            }

            columns.Add(column);
            if (column.IsPrimaryKey)
            {
                primaryKeys[tableName] = column.Name;
            }
        }

        return tableMap
            .OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => new DatabaseTableInfo
            {
                Name = x.Key,
                PrimaryKeyColumn = primaryKeys.GetValueOrDefault(x.Key),
                Columns = x.Value
            })
            .ToList();
    }

    public async Task<DatabaseTableRows> GetRowsAsync(string tableName, int limit, int offset, CancellationToken ct = default)
    {
        var table = await GetRequiredTableAsync(tableName, ct);
        var orderBy = table.PrimaryKeyColumn is not null
            ? $" ORDER BY {QuoteIdentifier(table.PrimaryKeyColumn)}"
            : string.Empty;
        var sql = $"""
            SELECT *
            FROM {QuoteIdentifier(table.Name)}
            {orderBy}
            LIMIT @limit OFFSET @offset;
            """;

        var rows = await ExecuteRowsQueryAsync(sql, table, ct, ("@limit", limit), ("@offset", offset));

        return new DatabaseTableRows
        {
            TableName = table.Name,
            PrimaryKeyColumn = table.PrimaryKeyColumn,
            Limit = limit,
            Offset = offset,
            Columns = table.Columns,
            Rows = rows
        };
    }

    public async Task<IReadOnlyDictionary<string, object?>?> GetRowByPrimaryKeyAsync(string tableName, string keyValue, CancellationToken ct = default)
    {
        var table = await GetRequiredTableAsync(tableName, ct);
        if (string.IsNullOrWhiteSpace(table.PrimaryKeyColumn))
        {
            throw new InvalidOperationException($"Table '{table.Name}' does not expose a primary key.");
        }

        var sql = $"""
            SELECT *
            FROM {QuoteIdentifier(table.Name)}
            WHERE {QuoteIdentifier(table.PrimaryKeyColumn)} = @keyValue
            LIMIT 1;
            """;

        var rows = await ExecuteRowsQueryAsync(sql, table, ct, ("@keyValue", keyValue));
        return rows.FirstOrDefault();
    }

    public async Task<DatabaseTableRows> GetRowsByColumnAsync(string tableName, string columnName, string value, int limit, CancellationToken ct = default)
    {
        var table = await GetRequiredTableAsync(tableName, ct);
        var column = table.Columns.FirstOrDefault(x => string.Equals(x.Name, columnName, StringComparison.OrdinalIgnoreCase))
                     ?? throw new InvalidOperationException($"Column '{columnName}' does not exist on table '{table.Name}'.");

        var sql = $"""
            SELECT *
            FROM {QuoteIdentifier(table.Name)}
            WHERE {QuoteIdentifier(column.Name)} = @value
            LIMIT @limit;
            """;

        var rows = await ExecuteRowsQueryAsync(sql, table, ct, ("@value", value), ("@limit", limit));

        return new DatabaseTableRows
        {
            TableName = table.Name,
            PrimaryKeyColumn = table.PrimaryKeyColumn,
            Limit = limit,
            Offset = 0,
            Columns = table.Columns,
            Rows = rows
        };
    }

    private async Task<DatabaseTableInfo> GetRequiredTableAsync(string tableName, CancellationToken ct)
    {
        var tables = await GetTablesAsync(ct);
        return tables.FirstOrDefault(x => string.Equals(x.Name, tableName, StringComparison.OrdinalIgnoreCase))
               ?? throw new InvalidOperationException($"Table '{tableName}' was not found in the active schema.");
    }

    private async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ExecuteRowsQueryAsync(
        string sql,
        DatabaseTableInfo table,
        CancellationToken ct,
        params (string Name, object? Value)[] parameters)
    {
        var rows = new List<IReadOnlyDictionary<string, object?>>();

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, conn);
        foreach (var parameter in parameters)
        {
            cmd.Parameters.AddWithValue(parameter.Name, parameter.Value);
        }

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(ReadRow(reader, table.Columns));
        }

        return rows;
    }

    private static IReadOnlyDictionary<string, object?> ReadRow(MySqlDataReader reader, IReadOnlyList<DatabaseColumnInfo> columns)
    {
        var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var column in columns)
        {
            var value = reader[column.Name];
            row[column.Name] = NormalizeValue(value);
        }

        return row;
    }

    private static object? NormalizeValue(object? value)
    {
        if (value is null || value is DBNull)
        {
            return null;
        }

        if (value is byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        return value;
    }

    private static string QuoteIdentifier(string identifier)
    {
        return $"`{identifier.Replace("`", "``", StringComparison.Ordinal)}`";
    }
}

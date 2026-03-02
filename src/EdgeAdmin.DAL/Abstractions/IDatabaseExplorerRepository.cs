namespace EdgeAdmin.DAL.Abstractions;

public interface IDatabaseExplorerRepository
{
    Task<IReadOnlyList<DatabaseTableInfo>> GetTablesAsync(CancellationToken ct = default);
    Task<DatabaseTableRows> GetRowsAsync(string tableName, int limit, int offset, CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, object?>?> GetRowByPrimaryKeyAsync(string tableName, string keyValue, CancellationToken ct = default);
    Task<DatabaseTableRows> GetRowsByColumnAsync(string tableName, string columnName, string value, int limit, CancellationToken ct = default);
}

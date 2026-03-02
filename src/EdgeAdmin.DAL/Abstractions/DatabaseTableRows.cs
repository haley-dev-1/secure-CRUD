namespace EdgeAdmin.DAL.Abstractions;

public sealed class DatabaseTableRows
{
    public string TableName { get; init; } = "";
    public string? PrimaryKeyColumn { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
    public IReadOnlyList<DatabaseColumnInfo> Columns { get; init; } = [];
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows { get; init; } = [];
}

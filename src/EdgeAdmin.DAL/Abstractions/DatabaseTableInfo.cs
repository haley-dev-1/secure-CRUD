namespace EdgeAdmin.DAL.Abstractions;

public sealed class DatabaseTableInfo
{
    public string Name { get; init; } = "";
    public string? PrimaryKeyColumn { get; init; }
    public IReadOnlyList<DatabaseColumnInfo> Columns { get; init; } = [];
}

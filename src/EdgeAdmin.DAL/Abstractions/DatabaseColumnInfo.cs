namespace EdgeAdmin.DAL.Abstractions;

public sealed class DatabaseColumnInfo
{
    public string Name { get; init; } = "";
    public string DataType { get; init; } = "";
    public bool IsNullable { get; init; }
    public bool IsPrimaryKey { get; init; }
}

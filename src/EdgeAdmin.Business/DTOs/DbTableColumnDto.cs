namespace EdgeAdmin.Business.DTOs;

public sealed class DbTableColumnDto
{
    public string Name { get; init; } = "";
    public string DataType { get; init; } = "";
    public bool IsNullable { get; init; }
    public bool IsPrimaryKey { get; init; }
}

namespace EdgeAdmin.Business.DTOs;

public sealed class DbTableRowsDto
{
    public string TableName { get; init; } = "";
    public string? PrimaryKeyColumn { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
    public IReadOnlyList<DbTableColumnDto> Columns { get; init; } = [];
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows { get; init; } = [];
}

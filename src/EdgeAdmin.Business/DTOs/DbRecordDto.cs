namespace EdgeAdmin.Business.DTOs;

public sealed class DbRecordDto
{
    public string TableName { get; init; } = "";
    public string? PrimaryKeyColumn { get; init; }
    public IReadOnlyList<DbTableColumnDto> Columns { get; init; } = [];
    public IReadOnlyDictionary<string, object?> Values { get; init; } = new Dictionary<string, object?>();
}

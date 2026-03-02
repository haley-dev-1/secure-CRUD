namespace EdgeAdmin.Business.DTOs;

public sealed class DbTableSummaryDto
{
    public string Name { get; init; } = "";
    public string? PrimaryKeyColumn { get; init; }
    public int ColumnCount { get; init; }
    public IReadOnlyList<DbTableColumnDto> Columns { get; init; } = [];
}

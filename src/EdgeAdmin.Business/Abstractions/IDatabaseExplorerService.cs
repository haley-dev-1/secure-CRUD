using EdgeAdmin.Business.DTOs;
using EdgeAdmin.Business.Results;

namespace EdgeAdmin.Business.Abstractions;

public interface IDatabaseExplorerService
{
    Task<ServiceResult<IReadOnlyList<DbTableSummaryDto>>> GetTablesAsync(CancellationToken ct = default);
    Task<ServiceResult<DbTableRowsDto>> GetRowsAsync(string tableName, int limit = 50, int offset = 0, CancellationToken ct = default);
    Task<ServiceResult<DbRecordDto>> GetRowByPrimaryKeyAsync(string tableName, string keyValue, CancellationToken ct = default);
    Task<ServiceResult<DbTableRowsDto>> GetRowsByColumnAsync(string tableName, string columnName, string value, int limit = 50, CancellationToken ct = default);
}

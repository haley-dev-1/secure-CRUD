using EdgeAdmin.Business.Abstractions;
using EdgeAdmin.Business.DTOs;
using EdgeAdmin.Business.Results;
using EdgeAdmin.DAL.Abstractions;

namespace EdgeAdmin.Business.Services;

public sealed class DatabaseExplorerService : IDatabaseExplorerService
{
    private readonly IDatabaseExplorerRepository _repository;

    public DatabaseExplorerService(IDatabaseExplorerRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<IReadOnlyList<DbTableSummaryDto>>> GetTablesAsync(CancellationToken ct = default)
    {
        try
        {
            var tables = await _repository.GetTablesAsync(ct);
            return ServiceResult<IReadOnlyList<DbTableSummaryDto>>.Success(
                tables.Select(ToSummaryDto).ToList());
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<IReadOnlyList<DbTableSummaryDto>>(ex);
        }
    }

    public async Task<ServiceResult<DbTableRowsDto>> GetRowsAsync(string tableName, int limit = 50, int offset = 0, CancellationToken ct = default)
    {
        if (!IsValidIdentifier(tableName))
        {
            return ServiceResult<DbTableRowsDto>.Failure(ErrorCodes.Validation, "Table name is required.");
        }

        if (limit <= 0 || limit > 200 || offset < 0)
        {
            return ServiceResult<DbTableRowsDto>.Failure(ErrorCodes.Validation, "Limit must be 1-200 and offset must be 0 or greater.");
        }

        try
        {
            var rows = await _repository.GetRowsAsync(tableName.Trim(), limit, offset, ct);
            return ServiceResult<DbTableRowsDto>.Success(ToRowsDto(rows));
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<DbTableRowsDto>.Failure(ErrorCodes.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<DbTableRowsDto>(ex);
        }
    }

    public async Task<ServiceResult<DbRecordDto>> GetRowByPrimaryKeyAsync(string tableName, string keyValue, CancellationToken ct = default)
    {
        if (!IsValidIdentifier(tableName) || string.IsNullOrWhiteSpace(keyValue))
        {
            return ServiceResult<DbRecordDto>.Failure(ErrorCodes.Validation, "Table name and primary key value are required.");
        }

        try
        {
            var tables = await _repository.GetTablesAsync(ct);
            var table = tables.FirstOrDefault(x => string.Equals(x.Name, tableName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (table is null)
            {
                return ServiceResult<DbRecordDto>.Failure(ErrorCodes.NotFound, $"Table '{tableName}' was not found.");
            }

            var row = await _repository.GetRowByPrimaryKeyAsync(table.Name, keyValue.Trim(), ct);
            if (row is null)
            {
                return ServiceResult<DbRecordDto>.Failure(ErrorCodes.NotFound, $"No record was found in '{table.Name}' for key '{keyValue}'.");
            }

            return ServiceResult<DbRecordDto>.Success(new DbRecordDto
            {
                TableName = table.Name,
                PrimaryKeyColumn = table.PrimaryKeyColumn,
                Columns = table.Columns.Select(ToColumnDto).ToList(),
                Values = row
            });
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<DbRecordDto>.Failure(ErrorCodes.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<DbRecordDto>(ex);
        }
    }

    public async Task<ServiceResult<DbTableRowsDto>> GetRowsByColumnAsync(string tableName, string columnName, string value, int limit = 50, CancellationToken ct = default)
    {
        if (!IsValidIdentifier(tableName) || !IsValidIdentifier(columnName) || string.IsNullOrWhiteSpace(value))
        {
            return ServiceResult<DbTableRowsDto>.Failure(ErrorCodes.Validation, "Table name, column name, and filter value are required.");
        }

        if (limit <= 0 || limit > 200)
        {
            return ServiceResult<DbTableRowsDto>.Failure(ErrorCodes.Validation, "Limit must be between 1 and 200.");
        }

        try
        {
            var rows = await _repository.GetRowsByColumnAsync(tableName.Trim(), columnName.Trim(), value.Trim(), limit, ct);
            return ServiceResult<DbTableRowsDto>.Success(ToRowsDto(rows));
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<DbTableRowsDto>.Failure(ErrorCodes.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<DbTableRowsDto>(ex);
        }
    }

    private static DbTableSummaryDto ToSummaryDto(DatabaseTableInfo source)
    {
        return new DbTableSummaryDto
        {
            Name = source.Name,
            PrimaryKeyColumn = source.PrimaryKeyColumn,
            ColumnCount = source.Columns.Count,
            Columns = source.Columns.Select(ToColumnDto).ToList()
        };
    }

    private static DbTableRowsDto ToRowsDto(DatabaseTableRows source)
    {
        return new DbTableRowsDto
        {
            TableName = source.TableName,
            PrimaryKeyColumn = source.PrimaryKeyColumn,
            Limit = source.Limit,
            Offset = source.Offset,
            Columns = source.Columns.Select(ToColumnDto).ToList(),
            Rows = source.Rows
        };
    }

    private static DbTableColumnDto ToColumnDto(DatabaseColumnInfo source)
    {
        return new DbTableColumnDto
        {
            Name = source.Name,
            DataType = source.DataType,
            IsNullable = source.IsNullable,
            IsPrimaryKey = source.IsPrimaryKey
        };
    }

    private static bool IsValidIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.All(ch => char.IsLetterOrDigit(ch) || ch == '_');
    }

    private static ServiceResult<T> ToInfrastructureFailure<T>(Exception ex)
    {
        return ServiceResult<T>.Failure(
            ErrorCodes.Infrastructure,
            $"Database explorer failed: {ex.Message}");
    }
}

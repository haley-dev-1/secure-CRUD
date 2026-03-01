using EdgeAdmin.Business.Abstractions;
using EdgeAdmin.Business.DTOs;
using EdgeAdmin.Business.Mapping;
using EdgeAdmin.Business.Results;
using EdgeAdmin.DAL.Abstractions;
using EdgeAdmin.Shared.Models;
using MySqlConnector;

namespace EdgeAdmin.Business.Services;

public sealed class DeviceQueryService : IDeviceQueryService
{
    private readonly IDeviceQueryRepository _repo;
    private readonly ICrudRepository<Device, long> _crud;
    private readonly IUserAccountRepository _users;

    public DeviceQueryService(
        IDeviceQueryRepository repo,
        ICrudRepository<Device, long> crud,
        IUserAccountRepository users)
    {
        _repo = repo;
        _crud = crud;
        _users = users;
    }

    public async Task<ServiceResult<IReadOnlyList<DeviceSummaryDto>>> GetAllDevicesAsync(CancellationToken ct = default)
    {
        try
        {
            var rows = await _repo.GetAllAsync(ct);
            var dto = rows.Select(DeviceTranslator.ToSummaryDto).ToList();
            return ServiceResult<IReadOnlyList<DeviceSummaryDto>>.Success(dto);
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<IReadOnlyList<DeviceSummaryDto>>(ex);
        }
    }

    public async Task<ServiceResult<DeviceSummaryDto>> GetDeviceByIdAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0)
        {
            return ServiceResult<DeviceSummaryDto>.Failure(
                ErrorCodes.Validation,
                "Device id must be a positive integer.");
        }

        try
        {
            var row = await _crud.GetByIdAsync(id, ct);
            if (row is null)
            {
                return ServiceResult<DeviceSummaryDto>.Failure(
                    ErrorCodes.NotFound,
                    $"Device {id} does not exist.");
            }

            return ServiceResult<DeviceSummaryDto>.Success(DeviceTranslator.ToSummaryDto(row));
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<DeviceSummaryDto>(ex);
        }
    }

    public async Task<ServiceResult<long>> CreateDeviceAsync(CreateDeviceRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.PublicDeviceId)
            || string.IsNullOrWhiteSpace(request.Name)
            || request.DeviceTypeId <= 0
            || request.OwnerUserId <= 0)
        {
            return ServiceResult<long>.Failure(
                ErrorCodes.Validation,
                "PublicDeviceId, Name, DeviceTypeId, and OwnerUserId are required.");
        }

        try
        {
            var ownerExists = await _users.ExistsAsync(request.OwnerUserId, ct);
            if (!ownerExists)
            {
                return ServiceResult<long>.Failure(
                    ErrorCodes.NotFound,
                    $"Owner user {request.OwnerUserId} does not exist.");
            }

            var entity = new Device
            {
                DeviceGuid = request.PublicDeviceId.Trim(),
                DisplayName = request.Name.Trim(),
                DeviceTypeId = request.DeviceTypeId,
                OwnerUserId = request.OwnerUserId,
                CreatedAtUtc = DateTime.UtcNow
            };

            var id = await _crud.CreateAsync(entity, ct);
            return ServiceResult<long>.Success(id);
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<long>(ex);
        }
    }

    public async Task<ServiceResult<bool>> UpdateDeviceAsync(UpdateDeviceRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.TargetPublicDeviceId)
            || string.IsNullOrWhiteSpace(request.PublicDeviceId)
            || string.IsNullOrWhiteSpace(request.Name))
        {
            return ServiceResult<bool>.Failure(
                ErrorCodes.Validation,
                "TargetPublicDeviceId, PublicDeviceId, and Name are required.");
        }

        try
        {
            var updated = await _repo.UpdateByPublicDeviceIdAsync(
                request.TargetPublicDeviceId.Trim(),
                request.PublicDeviceId.Trim(),
                request.Name.Trim(),
                ct);
            if (!updated)
            {
                return ServiceResult<bool>.Failure(
                    ErrorCodes.NotFound,
                    $"Device '{request.TargetPublicDeviceId}' does not exist.");
            }

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<bool>(ex);
        }
    }

    public async Task<ServiceResult<bool>> DeleteDeviceAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0)
        {
            return ServiceResult<bool>.Failure(
                ErrorCodes.Validation,
                "Device id must be a positive integer.");
        }

        try
        {
            var deleted = await _crud.DeleteAsync(id, ct);
            if (!deleted)
            {
                return ServiceResult<bool>.Failure(
                    ErrorCodes.NotFound,
                    $"Device {id} does not exist.");
            }

            return ServiceResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<bool>(ex);
        }
    }

    public async Task<ServiceResult<int>> GetTotalDevicesAsync(CancellationToken ct = default)
    {
        try
        {
            var total = await _repo.GetTotalDevicesAsync(ct);
            return ServiceResult<int>.Success(total);
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<int>(ex);
        }
    }

    public async Task<ServiceResult<int>> GetTotalUsersAsync(CancellationToken ct = default)
    {
        try
        {
            var total = await _repo.GetTotalUsersAsync(ct);
            return ServiceResult<int>.Success(total);
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<int>(ex);
        }
    }

    public async Task<ServiceResult<UserLastUsedDeviceDto>> GetLastUsedDeviceForUserAsync(long userId, CancellationToken ct = default)
    {
        if (userId <= 0)
        {
            return ServiceResult<UserLastUsedDeviceDto>.Failure(
                ErrorCodes.Validation,
                "User id must be a positive integer.");
        }

        try
        {
            var exists = await _users.ExistsAsync(userId, ct);
            if (!exists)
            {
                return ServiceResult<UserLastUsedDeviceDto>.Failure(
                    ErrorCodes.NotFound,
                    $"User {userId} does not exist.");
            }

            var device = await _repo.GetMostRecentDeviceByUserIdAsync(userId, ct);
            if (device is null)
            {
                return ServiceResult<UserLastUsedDeviceDto>.Failure(
                    ErrorCodes.NotFound,
                    $"No device usage found for user {userId}.");
            }

            return ServiceResult<UserLastUsedDeviceDto>.Success(
                DeviceTranslator.ToLastUsedDto(userId, device));
        }
        catch (MySqlException ex) when (IsLikelySchemaMismatch(ex))
        {
            return ServiceResult<UserLastUsedDeviceDto>.Failure(
                ErrorCodes.SchemaMismatch,
                "Schema does not support 'last used by user' yet. Expected devices.owner_user_id and devices.created_at.");
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<UserLastUsedDeviceDto>(ex);
        }
    }

    public async Task<ServiceResult<UserAccountDatesDto>> GetUserAccountDatesAsync(long userId, CancellationToken ct = default)
    {
        if (userId <= 0)
        {
            return ServiceResult<UserAccountDatesDto>.Failure(
                ErrorCodes.Validation,
                "User id must be a positive integer.");
        }

        try
        {
            var user = await _users.GetByIdAsync(userId, ct);
            if (user is null)
            {
                return ServiceResult<UserAccountDatesDto>.Failure(
                    ErrorCodes.NotFound,
                    $"User {userId} does not exist.");
            }

            return ServiceResult<UserAccountDatesDto>.Success(
                UserAccountTranslator.ToDatesDto(user));
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<UserAccountDatesDto>(ex);
        }
    }

    public async Task<ServiceResult<UserAccountDatesDto>> MarkUserInactiveIfStaleAsync(long userId, CancellationToken ct = default)
    {
        if (userId <= 0)
        {
            return ServiceResult<UserAccountDatesDto>.Failure(
                ErrorCodes.Validation,
                "User id must be a positive integer.");
        }

        try
        {
            var user = await _users.GetByIdAsync(userId, ct);
            if (user is null)
            {
                return ServiceResult<UserAccountDatesDto>.Failure(
                    ErrorCodes.NotFound,
                    $"User {userId} does not exist.");
            }

            // Business rule: if updated_at is in a prior calendar month, mark inactive.
            // Example: updated in January, checked in February => inactive.
            var now = DateTime.Now;
            var monthsDiff = (now.Year - user.UpdatedAtUtc.Year) * 12 + (now.Month - user.UpdatedAtUtc.Month);
            var isStale = monthsDiff >= 1;
            var inactiveStatusValue = await _users.ResolveInactiveStatusValueAsync(ct);

            if (isStale && !string.Equals(user.Status, inactiveStatusValue, StringComparison.OrdinalIgnoreCase))
            {
                await _users.UpdateStatusAsync(userId, inactiveStatusValue, ct);
            }

            // Re-read to return the current canonical user row shape
            // (same structure as GetUserAccountDatesAsync).
            var refreshed = await _users.GetByIdAsync(userId, ct);
            if (refreshed is null)
            {
                return ServiceResult<UserAccountDatesDto>.Failure(
                    ErrorCodes.Infrastructure,
                    $"User {userId} could not be reloaded after status check.");
            }

            return ServiceResult<UserAccountDatesDto>.Success(
                UserAccountTranslator.ToDatesDto(refreshed));
        }
        catch (Exception ex)
        {
            return ToInfrastructureFailure<UserAccountDatesDto>(ex);
        }
    }

    private static bool IsLikelySchemaMismatch(MySqlException ex)
    {
        var msg = ex.Message;
        return msg.Contains("Unknown column", StringComparison.OrdinalIgnoreCase)
               || msg.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase)
               || msg.Contains("Table", StringComparison.OrdinalIgnoreCase);
    }

    private static ServiceResult<T> ToInfrastructureFailure<T>(Exception ex)
    {
        return ServiceResult<T>.Failure(
            ErrorCodes.Infrastructure,
            $"Data access failed: {ex.Message}");
    }
}

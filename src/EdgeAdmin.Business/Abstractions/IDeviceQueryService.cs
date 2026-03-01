using EdgeAdmin.Business.DTOs;
using EdgeAdmin.Business.Results;

namespace EdgeAdmin.Business.Abstractions;

public interface IDeviceQueryService
{
    Task<ServiceResult<IReadOnlyList<DeviceSummaryDto>>> GetAllDevicesAsync(CancellationToken ct = default);
    Task<ServiceResult<DeviceSummaryDto>> GetDeviceByIdAsync(long id, CancellationToken ct = default);
    Task<ServiceResult<long>> CreateDeviceAsync(CreateDeviceRequestDto request, CancellationToken ct = default);
    Task<ServiceResult<bool>> UpdateDeviceAsync(UpdateDeviceRequestDto request, CancellationToken ct = default);
    Task<ServiceResult<bool>> DeleteDeviceAsync(long id, CancellationToken ct = default);
    Task<ServiceResult<int>> GetTotalDevicesAsync(CancellationToken ct = default);
    Task<ServiceResult<int>> GetTotalUsersAsync(CancellationToken ct = default);
    Task<ServiceResult<UserLastUsedDeviceDto>> GetLastUsedDeviceForUserAsync(long userId, CancellationToken ct = default);
    Task<ServiceResult<UserAccountDatesDto>> GetUserAccountDatesAsync(long userId, CancellationToken ct = default);
    Task<ServiceResult<UserAccountDatesDto>> MarkUserInactiveIfStaleAsync(long userId, CancellationToken ct = default);
}

using EdgeAdmin.Shared.Models;

namespace EdgeAdmin.DAL.Abstractions;

public interface IDeviceQueryRepository
{
    Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken ct = default);
    Task<bool> UpdateByPublicDeviceIdAsync(string targetPublicDeviceId, string newPublicDeviceId, string name, CancellationToken ct = default);
    Task<int> GetTotalDevicesAsync(CancellationToken ct = default);
    Task<int> GetTotalUsersAsync(CancellationToken ct = default);
    Task<Device?> GetMostRecentDeviceByUserIdAsync(long userId, CancellationToken ct = default);
}

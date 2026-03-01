using EdgeAdmin.Shared.Models;

namespace EdgeAdmin.DAL.Abstractions;

public interface IUserAccountRepository
{
    Task<bool> ExistsAsync(long userId, CancellationToken ct = default);
    Task<UserAccount?> GetByIdAsync(long userId, CancellationToken ct = default);
    Task<bool> UpdateStatusAsync(long userId, string status, CancellationToken ct = default);
    Task<string> ResolveInactiveStatusValueAsync(CancellationToken ct = default);
}

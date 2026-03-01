using EdgeAdmin.Business.DTOs;
using EdgeAdmin.Shared.Models;

namespace EdgeAdmin.Business.Mapping;

/// <summary>
/// TRANSLATOR BOX (Intentional anti-coupling layer):
/// - UI must not depend on DB column names or DAL entity structure.
/// - DAL entities can change when schema changes.
/// - UI DTO contracts should stay stable for callers.
///
/// When DB schema changes, update mapping in this file first.
/// This gives you one obvious place to adapt data shape safely.
/// </summary>
public static class DeviceTranslator
{
    /// <summary>
    /// Maps a DAL/shared Device entity into a UI-safe DTO.
    /// </summary>
    public static DeviceSummaryDto ToSummaryDto(Device source)
    {
        return new DeviceSummaryDto
        {
            Id = source.DeviceId,
            PublicDeviceId = source.DeviceGuid,
            Name = source.DisplayName,
            CreatedAtUtc = source.CreatedAtUtc
        };
    }

    /// <summary>
    /// Maps a device selected for a user's "last used" request.
    /// We currently use the device CreatedAtUtc value as a last-used proxy.
    /// If your schema later adds a real last_used_at column, change only this mapping
    /// and the DAL query method, not the UI contract.
    /// </summary>
    public static UserLastUsedDeviceDto ToLastUsedDto(long userId, Device source)
    {
        return new UserLastUsedDeviceDto
        {
            UserId = userId,
            DeviceId = source.DeviceId,
            DeviceName = source.DisplayName,
            LastUsedAtUtc = source.CreatedAtUtc
        };
    }
}

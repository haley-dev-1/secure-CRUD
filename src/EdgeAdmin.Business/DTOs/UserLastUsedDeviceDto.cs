namespace EdgeAdmin.Business.DTOs;

public sealed class UserLastUsedDeviceDto
{
    public long UserId { get; init; }
    public long DeviceId { get; init; }
    public string DeviceName { get; init; } = "";
    public DateTime LastUsedAtUtc { get; init; }
}

namespace EdgeAdmin.Business.DTOs;

public sealed class DeviceSummaryDto
{
    public long Id { get; init; }
    public string PublicDeviceId { get; init; } = "";
    public string Name { get; init; } = "";
    public DateTime CreatedAtUtc { get; init; }
}

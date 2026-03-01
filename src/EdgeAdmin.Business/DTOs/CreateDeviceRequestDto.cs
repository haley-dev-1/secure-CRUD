namespace EdgeAdmin.Business.DTOs;

public sealed class CreateDeviceRequestDto
{
    public string PublicDeviceId { get; init; } = "";
    public string Name { get; init; } = "";
    public long DeviceTypeId { get; init; }
    public long OwnerUserId { get; init; }
}

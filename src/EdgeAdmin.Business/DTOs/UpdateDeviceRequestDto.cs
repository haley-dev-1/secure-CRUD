namespace EdgeAdmin.Business.DTOs;

public sealed class UpdateDeviceRequestDto
{
    public string TargetPublicDeviceId { get; init; } = "";
    public string PublicDeviceId { get; init; } = "";
    public string Name { get; init; } = "";
}

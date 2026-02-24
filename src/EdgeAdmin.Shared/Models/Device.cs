namespace EdgeAdmin.Shared.Models;

public sealed class Device
{
    public long DeviceId { get; set; }            // PK
    public string DeviceGuid { get; set; } = "";  // e.g. char(36) or varchar
    public string DisplayName { get; set; } = "";
    public long DeviceTypeId { get; set; }
    public long OwnerUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }    // if you have it; otherwise remove
}

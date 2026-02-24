-- Replace table_name with your real table:
-- SELECT COUNT(*) AS row_count FROM table_name;

namespace EdgeAdmin.Shared.Models;

public sealed class Device
{
    public long DeviceId { get; set; }            // PK
    public string DeviceGuid { get; set; } = "";  // unique-ish external id
    public string DisplayName { get; set; } = ""; // friendly name
    public long? UserAccountId { get; set; }      // FK (nullable if unassigned)
    public DateTime CreatedAtUtc { get; set; }
}

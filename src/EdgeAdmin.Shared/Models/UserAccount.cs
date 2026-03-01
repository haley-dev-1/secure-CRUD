namespace EdgeAdmin.Shared.Models;

public sealed class UserAccount
{
    public long UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string Status { get; set; } = "";
}

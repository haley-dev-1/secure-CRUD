namespace EdgeAdmin.Business.DTOs;

public sealed class UserAccountDatesDto
{
    public long UserId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public string Status { get; init; } = "";
}

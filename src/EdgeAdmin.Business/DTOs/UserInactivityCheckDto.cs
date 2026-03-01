namespace EdgeAdmin.Business.DTOs;

public sealed class UserInactivityCheckDto
{
    public long UserId { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public DateTime CheckedAtUtc { get; init; }
    public string PreviousStatus { get; init; } = "";
    public string CurrentStatus { get; init; } = "";
    public bool MarkedInactive { get; init; }
}

using EdgeAdmin.Business.DTOs;
using EdgeAdmin.Shared.Models;

namespace EdgeAdmin.Business.Mapping;

/// <summary>
/// Translator for user-account related DTOs.
/// Keeps UI/service contracts stable when the user_accounts schema evolves.
/// </summary>
public static class UserAccountTranslator
{
    public static UserAccountDatesDto ToDatesDto(UserAccount source)
    {
        return new UserAccountDatesDto
        {
            UserId = source.UserId,
            CreatedAtUtc = source.CreatedAtUtc,
            UpdatedAtUtc = source.UpdatedAtUtc,
            Status = source.Status
        };
    }
}

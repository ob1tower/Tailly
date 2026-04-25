namespace Tailly.AuthService.Application.Dtos.Responses.AccountDeletion;

public sealed class RestorePreviewResponse
{
    public string Email { get; set; } = default!;
    public string RoleLabel { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public DateTime RestoreDeadlineIso { get; set; }
}
namespace Tailly.AuthService.Application.Dtos.Responses.AccountDeletion;

public sealed class DeletionResponse
{
    public bool Ok { get; set; }
    public DateTime RestoreDeadlineIso { get; set; }
}
namespace Tailly.AuthService.Application.Dtos.Responses;

public sealed class StartPasswordRecoveryResponse
{
    public string Flow { get; set; } = "default";
}
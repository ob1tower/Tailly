namespace Tailly.AuthService.Application.Dtos.Responses;

public sealed class EmailChangeResponse
{
    public string RequestId { get; set; } = default!;
    public string MaskedOldEmail { get; set; } = default!;
}
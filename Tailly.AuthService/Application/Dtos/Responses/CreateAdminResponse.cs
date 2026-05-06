using Tailly.AuthService.Core.Models;

namespace Tailly.AuthService.Application.Dtos.Responses;

public class CreateAdminResponse
{
    public ManagedAdmin Admin { get; set; } = default!;
    public string TemporaryPassword { get; set; } = default!;
}
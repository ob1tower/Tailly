namespace Tailly.AuthService.Application.Dtos.Common;

public sealed class AuthUserDto
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string? SpecialistSlug { get; set; }

    public string? SpecialistId { get; set; }
    public string? AdminId { get; set; }
}
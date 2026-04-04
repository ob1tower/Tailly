namespace Tailly.AuthService.Application.Dtos.Common;

public sealed class AuthUserDto
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public string? SpecialistSlug { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? CityId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? SpecialistId { get; set; }
    public string? AdminId { get; set; }
}
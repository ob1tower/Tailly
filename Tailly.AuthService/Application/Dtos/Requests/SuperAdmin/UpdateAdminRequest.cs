namespace Tailly.AuthService.Application.Dtos.Requests.SuperAdmin;

public sealed class UpdateAdminRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public DateTime BirthDate { get; set; }
    public string? Phone { get; set; }
    public string? Department { get; set; }
}
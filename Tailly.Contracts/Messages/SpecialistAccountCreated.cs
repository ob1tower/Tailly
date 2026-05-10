namespace Tailly.Contracts.Messages;

public record SpecialistAccountCreated
{
    public Guid ApplicationId { get; init; }
    public Guid SpecialistId { get; init; }
    public string? Slug { get; set; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? MiddleName { get; init; }
    public string Phone { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string About { get; init; } = string.Empty;
    public string? TemporaryPassword { get; init; }
    public string CreatedByAdminId { get; init; } = string.Empty;
}
namespace Tailly.Contracts.Messages;

public record class SpecialistUserLinked
{
    public Guid SpecialistId { get; set; }
    public Guid UserId { get; set; }
}
namespace Tailly.Contracts.Messages;

public sealed class OrderCompletedMessage
{
    public Guid OrderId { get; set; }
    public Guid SpecialistId { get; set; }
    public bool IsRepeatOrder { get; set; }
}
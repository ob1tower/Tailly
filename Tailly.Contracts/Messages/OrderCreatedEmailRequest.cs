namespace Tailly.Contracts.Messages;

public record OrderCreatedEmailRequest
{
    public Guid OrderId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string OrderNumber { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public List<OrderEmailItem> Items { get; init; } = [];
}
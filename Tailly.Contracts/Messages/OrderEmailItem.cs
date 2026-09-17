namespace Tailly.Contracts.Messages;

public record OrderEmailItem
{
    public string Title { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
    public decimal LineTotal { get; init; }
}
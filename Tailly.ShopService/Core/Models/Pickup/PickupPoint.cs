using Tailly.ShopService.Core.Enums;

namespace Tailly.ShopService.Core.Models.Pickup;

public class PickupPoint
{
    public Guid Id { get; set; }
    public PickupProvider Provider { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime EstimatedDate { get; set; }
}
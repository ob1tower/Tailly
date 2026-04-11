using Tailly.ShopService.Core.Enums;

namespace Tailly.ShopService.Core.Entities.Pickup;

public class PickupPointEntity
{
    public Guid Id { get; set; }
    public PickupProvider Provider { get; set; } = PickupProvider.Cdek;
    public string Title { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime EstimatedDate { get; set; }
}
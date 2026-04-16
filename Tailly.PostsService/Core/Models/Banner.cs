using Tailly.PostsService.Core.Enums;

namespace Tailly.PostsService.Core.Models;

public class Banner
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public BannerLinkTarget LinkTarget { get; set; }     
    public BannerPlacement Placement { get; set; }        
    public BannerStatus Status { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
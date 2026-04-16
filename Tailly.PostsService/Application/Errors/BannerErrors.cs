using Tailly.PostsService.Core.Common;

namespace Tailly.PostsService.Application.Errors;

public static class BannerErrors
{
    public static readonly Error BannerNotFound =
        new("Banner.NotFound", "Banner not found.");

    public static readonly Error InvalidBanner =
        new("Banner.Invalid", "Banner is invalid.");

    public static readonly Error EmptyTitle =
        new("Banner.EmptyTitle", "Title cannot be empty.");

    public static readonly Error EmptyDescription =
        new("Banner.EmptyDescription", "Description cannot be empty.");

    public static readonly Error InvalidPlacement = 
        new("Banner.InvalidPlacement", "Invalid banner placement.");

    public static readonly Error InvalidSort =
        new("Banner.InvalidSort", "Invalid sort value.");
}
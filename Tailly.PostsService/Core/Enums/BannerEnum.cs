namespace Tailly.PostsService.Core.Enums;

public enum BannerStatus
{
    Draft = 1,
    Published,
    Archived
}

public enum BannerLinkTarget
{
    Home = 1,
    Posts,
    Specialists,
    Shop,
    Profile
}

public enum BannerPlacement
{
    HomeHero = 1,
    Posts,
    Specialists,
    Shop
}

public enum BannerSort
{
    Newest = 1,
    Oldest,
    TitleAsc,
    TitleDesc,
    StartsAtAsc,
    StartsAtDesc
}
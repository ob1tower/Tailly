namespace Tailly.PostsService.Core.Enums;

public enum PostStatus
{
    Draft = 1,
    Published,
    Archived
}

public enum PostPublicSort
{
    Newest = 1,
    Oldest,
    TitleAsc,
    TitleDesc
}

public enum PostAdminSort
{
    UpdatedDesc,
    UpdatedAsc,
    TitleAsc,
    TitleDesc,
    PublishedDesc
}
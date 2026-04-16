using Tailly.PostsService.Core.Common;

namespace Tailly.PostsService.Application.Errors;

public static class PostErrors
{
    public static readonly Error PostNotFound =
        new("Post.NotFound", "Post not found.");

    public static readonly Error InvalidPost =
        new("Post.Invalid", "Post is invalid.");

    public static readonly Error EmptyTitle =
        new("Post.EmptyTitle", "Title cannot be empty.");

    public static readonly Error EmptyContent =
        new("Post.EmptyContent", "Content cannot be empty.");

    public static readonly Error InvalidImage =
        new("Post.Invalid.Image", "Invalid image file.");

    public static readonly Error ImageTooLarge =
        new("Post.Image.too.Large", "Image size exceeds limit.");

    public static readonly Error UploadFailed =
        new("Post.Upload.Failed", "Failed to upload image.");

    public static readonly Error InvalidSort =
        new("Post.InvalidSort", "Invalid sort value.");
}
namespace Tailly.PostsService.Application.Service.Interfaces;

public interface IMediaService
{
    Task<string> SaveAsync(IFormFile file, string folder);
}
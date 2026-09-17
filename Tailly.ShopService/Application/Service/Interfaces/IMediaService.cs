using CSharpFunctionalExtensions;
using Tailly.ShopService.Core.Common;

namespace Tailly.ShopService.Application.Service.Interfaces
{
    public interface IMediaService
    {
        Task<Result<string, Error>> UploadAsync(IFormFile file, string? mediaType, Guid userId);
    }
}
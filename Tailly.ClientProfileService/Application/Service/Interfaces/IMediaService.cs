using CSharpFunctionalExtensions;
using Tailly.ClientProfileService.Core.Common;

namespace Tailly.ClientProfileService.Application.Service.Interfaces
{
    public interface IMediaService
    {
        Task<Result<string, Error>> UploadAsync(IFormFile file, string? mediaType, Guid userId);
    }
}
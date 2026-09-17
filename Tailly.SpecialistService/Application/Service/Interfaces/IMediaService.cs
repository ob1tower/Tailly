using CSharpFunctionalExtensions;
using Tailly.SpecialistService.Core.Common;

namespace Tailly.SpecialistService.Application.Service.Interfaces
{
    public interface IMediaService
    {
        Task<Result<string, Error>> UploadAsync(IFormFile file, string? mediaType, Guid specialistId);
    }
}
using CSharpFunctionalExtensions;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Models.User;

namespace Tailly.ShopService.Application.Service.Interfaces;

public interface IFavoriteService
{
    Task<Result> AddAsync(Guid? userId, Guid? sessionId, Guid productId);
    Task<Result> ClearAsync(Guid? userId, Guid? sessionId);
    Task<Result<List<FavoriteItem>, Error>> GetAsync(Guid? userId, Guid? sessionId);
    Task<Result> RemoveAsync(Guid? userId, Guid? sessionId, Guid productId);
    Task<Result> MergeAsync(Guid userId, Guid sessionId);
}
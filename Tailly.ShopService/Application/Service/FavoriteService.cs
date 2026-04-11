using CSharpFunctionalExtensions;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Core.Common;
using Tailly.ShopService.Core.Models.Cart;
using Tailly.ShopService.Core.Models.User;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Application.Service;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(IFavoriteRepository favoriteRepository,
                           IProductRepository productRepository,
                           ICartRepository cartRepository,
                           ILogger<FavoriteService> logger)
    {
        _favoriteRepository = favoriteRepository;
        _productRepository = productRepository;
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task<Result> AddAsync(Guid? userId, Guid? sessionId, Guid productId)
    {
        if (userId == null && sessionId == null)
            return Result.Failure(ShopErrors.FavoriteIdentityRequired.Description);

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            return Result.Failure(ShopErrors.ProductNotFound.Description);

        if (userId != null)
        {
            var exists = await _favoriteRepository.ExistsAsync(userId, null, productId);

            if (!exists)
            {
                await _favoriteRepository.AddAsync(new Favorite
                {
                    UserId = userId,
                    SessionId = null,
                    ProductId = productId,
                    AddedAt = DateTime.UtcNow
                });
            }

            return Result.Success();
        }

        var existsSession = await _favoriteRepository.ExistsAsync(null, sessionId, productId);

        if (!existsSession)
        {
            await _favoriteRepository.AddAsync(new Favorite
            {
                UserId = null,
                SessionId = sessionId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            });
        }

        return Result.Success();
    }

    public async Task<Result<List<FavoriteItem>, Error>> GetAsync(Guid? userId, Guid? sessionId)
    {
        if (userId == null && sessionId == null)
            return Result.Failure<List<FavoriteItem>, Error>(ShopErrors.FavoriteIdentityRequired);

        if (userId != null && sessionId != null)
        {
            var guestFavorites = await _favoriteRepository.GetAsync(null, sessionId);

            if (guestFavorites.Any())
            {
                foreach (var fav in guestFavorites)
                {
                    var exists = await _favoriteRepository.ExistsAsync(userId, null, fav.ProductId);

                    if (!exists)
                    {
                        await _favoriteRepository.AddAsync(new Favorite
                        {
                            UserId = userId,
                            SessionId = null,
                            ProductId = fav.ProductId,
                            AddedAt = DateTime.UtcNow
                        });
                    }
                }

                await _favoriteRepository.ClearAsync(null, sessionId);
            }
        }

        var favorites = await _favoriteRepository.GetAsync(userId, sessionId);

        if (favorites.Count == 0)
            return Result.Success<List<FavoriteItem>, Error>(new List<FavoriteItem>());

        var productIds = favorites.Select(x => x.ProductId).ToList();
        var products = await _productRepository.GetByIdsAsync(productIds);
        var productMap = products.ToDictionary(x => x.Id);

        Cart? cart = null;

        if (userId != null)
            cart = await _cartRepository.GetByUserIdAsync(userId.Value);
        else if (sessionId != null)
            cart = await _cartRepository.GetBySessionIdAsync(sessionId.Value);

        var cartIds = cart?.Items
            .Select(x => x.ProductId)
            .ToHashSet() ?? new HashSet<Guid>();

        var result = new List<FavoriteItem>();

        foreach (var fav in favorites)
        {
            if (!productMap.TryGetValue(fav.ProductId, out var product))
                continue;

            result.Add(new FavoriteItem
            {
                ProductId = product.Id,
                Title = product.Title,
                Slug = product.Slug,
                Price = product.Price,
                OldPrice = product.OldPrice,
                ImageUrl = product.Images?.FirstOrDefault()?.Url,
                InCart = cartIds.Contains(product.Id)
            });
        }

        return Result.Success<List<FavoriteItem>, Error>(result);
    }

    public async Task<Result> RemoveAsync(Guid? userId, Guid? sessionId, Guid productId)
    {
        if (userId == null && sessionId == null)
        {
            _logger.LogWarning("RemoveFavorite failed: no identity provided");
            return Result.Failure(ShopErrors.FavoriteIdentityRequired.Description);
        }

        if (userId != null)
            await _favoriteRepository.RemoveAsync(userId, null, productId);

        if (sessionId != null)
            await _favoriteRepository.RemoveAsync(null, sessionId, productId);

        _logger.LogInformation("Product {ProductId} removed from favorites", productId);

        return Result.Success();
    }

    public async Task<Result> ClearAsync(Guid? userId, Guid? sessionId)
    {
        if (userId == null && sessionId == null)
        {
            _logger.LogWarning("ClearFavorites failed: no identity provided");
            return Result.Failure(ShopErrors.FavoriteIdentityRequired.Description);
        }

        if (userId != null)
            await _favoriteRepository.ClearAsync(userId, null);

        if (sessionId != null)
            await _favoriteRepository.ClearAsync(null, sessionId);

        _logger.LogInformation("Favorites cleared for user/session");

        return Result.Success();
    }

    public async Task<Result> MergeAsync(Guid userId, Guid sessionId)
    {
        var guestFavorites = await _favoriteRepository.GetAsync(null, sessionId);

        if (!guestFavorites.Any())
            return Result.Success();

        foreach (var fav in guestFavorites)
        {
            var exists = await _favoriteRepository.ExistsAsync(userId, null, fav.ProductId);

            if (!exists)
            {
                await _favoriteRepository.AddAsync(new Favorite
                {
                    UserId = userId,
                    SessionId = null,
                    ProductId = fav.ProductId,
                    AddedAt = DateTime.UtcNow
                });
            }
        }

        await _favoriteRepository.ClearAsync(null, sessionId);

        _logger.LogInformation("Favorites merged from session {SessionId} to user {UserId}", sessionId, userId);

        return Result.Success();
    }
}
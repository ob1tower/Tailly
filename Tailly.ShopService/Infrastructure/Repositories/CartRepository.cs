using Microsoft.EntityFrameworkCore;
using Tailly.ShopService.Core.Entities.Cart;
using Tailly.ShopService.Core.Models.Cart;
using Tailly.ShopService.Infrastructure.DataAccess;
using Tailly.ShopService.Infrastructure.Mappers;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ShopDbContext _context;

    public CartRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        var cartEntity = await _context.Carts
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return cartEntity?.ToDomain();
    }

    public async Task<Cart?> GetBySessionIdAsync(Guid sessionId)
    {
        var cartEntity = await _context.Carts
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionId == sessionId);

        return cartEntity?.ToDomain();
    }

    public async Task<Cart> CreateAsync(Guid? userId, Guid? sessionId)
    {
        var cartEntity = new CartEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Carts.AddAsync(cartEntity);
        await _context.SaveChangesAsync();

        return new Cart
        {
            Id = cartEntity.Id,
            UserId = cartEntity.UserId,
            SessionId = cartEntity.SessionId,
            Items = new List<CartItem>()
        };
    }

    public async Task AddItemAsync(Guid cartId, CartItem item)
    {
        var exists = await _context.CartItems
            .AnyAsync(x => x.CartId == cartId && x.ProductId == item.ProductId);

        if (exists)
            return;

        var cartItemEntity = item.ToEntity(cartId);

        await _context.CartItems.AddAsync(cartItemEntity);

        var cart = await _context.Carts.FindAsync(cartId);
        if (cart != null)
            cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task AssignUserAsync(Guid cartId, Guid userId)
    {
        var cart = await _context.Carts.FindAsync(cartId);
        if (cart == null) return;

        cart.UserId = userId;
        cart.SessionId = null;
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task MergeAsync(Guid targetCartId, Cart sourceCart)
    {
        var target = await _context.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == targetCartId);

        if (target == null) return;

        foreach (var sourceItem in sourceCart.Items)
        {
            var existingItem = target.Items.FirstOrDefault(x => x.ProductId == sourceItem.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += sourceItem.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var newItem = sourceItem.ToEntity(targetCartId);
                await _context.CartItems.AddAsync(newItem);
            }
        }

        target.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Guid cartId, Guid productId, int quantity)
    {
        var cartEntity = await _context.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cartId);

        if (cartEntity == null) return;

        var item = cartEntity.Items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null) return;

        item.Quantity = quantity;
        item.UpdatedAt = DateTime.UtcNow;
        cartEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(Guid cartId, Guid productId)
    {
        var cartEntity = await _context.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cartId);

        if (cartEntity == null) return;

        var item = cartEntity.Items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null) return;

        cartEntity.Items.Remove(item);
        cartEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task ClearAsync(Guid cartId)
    {
        var cartEntity = await _context.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == cartId);

        if (cartEntity == null) return;

        cartEntity.Items.Clear();
        cartEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteBySessionIdAsync(Guid sessionId)
    {
        var cart = await _context.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.SessionId == sessionId);

        if (cart == null) return;

        _context.CartItems.RemoveRange(cart.Items);
        _context.Carts.Remove(cart);

        await _context.SaveChangesAsync();
    }
}
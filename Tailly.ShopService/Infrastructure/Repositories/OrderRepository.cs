using Microsoft.EntityFrameworkCore;
using Tailly.ShopService.Core.Enums;
using Tailly.ShopService.Core.Models.Order;
using Tailly.ShopService.Infrastructure.DataAccess;
using Tailly.ShopService.Infrastructure.Mappers;
using Tailly.ShopService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ShopService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ShopDbContext _context;

    public OrderRepository(ShopDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        var entity = OrderEntityMapper.ToEntity(order);

        await _context.Orders.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Address)
            .Include(o => o.PickupPoint)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        return entity == null ? null : OrderEntityMapper.ToDomain(entity);
    }

    public async Task<List<Order>> GetByUserIdAsync(Guid userId)
    {
        var entities = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Address)
            .Include(o => o.PickupPoint)
            .AsNoTracking()
            .Where(o => o.OwnerUserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return entities.Select(OrderEntityMapper.ToDomain).ToList();
    }

    public async Task CancelAsync(Guid orderId)
    {
        var entity = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (entity == null)
            return;

        entity.Status = OrderStatus.Cancelled;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        var existingEntity = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Address)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        if (existingEntity == null)
            return;

        existingEntity.Status = order.Status;
        existingEntity.EstimatedDeliveryDate = order.EstimatedDeliveryDate;
        existingEntity.TotalPrice = order.TotalPrice; 

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Orders.AnyAsync(o => o.Id == id);
    }
}
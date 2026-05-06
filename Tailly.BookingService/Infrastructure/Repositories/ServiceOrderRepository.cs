using Microsoft.EntityFrameworkCore;
using Tailly.BookingService.Core.Entities;
using Tailly.BookingService.Core.Enums;
using Tailly.BookingService.Core.Models;
using Tailly.BookingService.Infrastructure.DataAccess;
using Tailly.BookingService.Infrastructure.Mappers;

namespace Tailly.BookingService.Infrastructure.Repositories;

public class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly BookingDbContext _context;

    public ServiceOrderRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ServiceOrder order)
    {
        var entity = ServiceOrderEntityMapper.ToEntity(order);

        await _context.ServiceOrders.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<ServiceOrder?> GetByIdAsync(Guid id)
    {
        var entity = await _context.ServiceOrders
            .Include(x => x.Review)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return entity == null ? null : ServiceOrderEntityMapper.ToDomain(entity);
    }

    public async Task<List<ServiceOrder>> GetByClientIdAsync(Guid clientId, string? statusFilter = null, int page = 1, int limit = 20)
    {
        var query = _context.ServiceOrders
                .Include(x => x.Review)
                .AsNoTracking()
                .Where(x => x.ClientId == clientId);

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            if (Enum.TryParse<OrderStatus>(statusFilter, true, out var status))
            {
                query = query.Where(x => x.Status == status);
            }
        }

        var entities = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return entities.Select(ServiceOrderEntityMapper.ToDomain).ToList();
    }

    public async Task<List<ServiceOrder>> GetBySpecialistIdAsync(Guid specialistId, string? statusFilter = null, int page = 1, int limit = 20)
    {
        var query = _context.ServiceOrders
            .Include(x => x.Review)
            .AsNoTracking()
            .Where(x => x.SpecialistId == specialistId);

        if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter.ToLowerInvariant() != "all")
        {
            if (Enum.TryParse<OrderStatus>(statusFilter, true, out var status))
            {
                query = query.Where(x => x.Status == status);
            }
        }

        var entities = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return entities.Select(ServiceOrderEntityMapper.ToDomain).ToList();
    }

    public async Task UpdateAsync(ServiceOrder order)
    {
        var entity = await _context.ServiceOrders
            .FirstOrDefaultAsync(x => x.Id == order.Id);

        if (entity == null)
            return;

        entity.Status = order.Status;
        entity.ConfirmedAt = order.ConfirmedAt;
        entity.StartedAt = order.StartedAt;
        entity.CompletedAt = order.CompletedAt;
        entity.CanceledAt = order.CanceledAt;
        entity.CancelReason = order.CancelReason;
        entity.Comment = order.Comment;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.ServiceOrders.AnyAsync(x => x.Id == id);
    }
}
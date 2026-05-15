using Microsoft.EntityFrameworkCore;
using Tailly.SpecialistService.Core.Entities.Calendar;
using Tailly.SpecialistService.Core.Models.Calendars;
using Tailly.SpecialistService.Infrastructure.DataAccess;
using Tailly.SpecialistService.Infrastructure.Mappers;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Infrastructure.Repositories;

public class CalendarRepository : ICalendarRepository
{
    private readonly SpecialistDbContext _context;

    public CalendarRepository(SpecialistDbContext context)
    {
        _context = context;
    }

    public async Task<Calendar?> GetAsync(Guid specialistId)
    {
        var entity = await _context.Calendars
            .Include(x => x.DayOverrides)
            .Include(x => x.BookedSlots)
            .Include(x => x.AvailabilityWindows)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SpecialistId == specialistId);

        if (entity == null)
            return null;

        return CalendarEntityMapper.ToModel(entity);
    }

    public async Task SaveAvailabilityWindowAsync(Guid specialistId, CalendarAvailabilityWindow window)
    {
        var calendar = await GetOrCreateCalendarAsync(specialistId);

        if (window.Id == Guid.Empty)
        {
            var entity = new CalendarAvailabilityWindowEntity
            {
                Id = Guid.NewGuid(),
                CalendarId = calendar.Id,
                Date = window.Date,
                StartTime = window.StartTime,
                EndTime = window.EndTime,
                ServiceId = window.ServiceId
            };

            await _context.CalendarAvailabilityWindows.AddAsync(entity);
        }
        else
        {
            var entity = await _context.CalendarAvailabilityWindows
                .FirstOrDefaultAsync(x => x.Id == window.Id);

            if (entity == null)
                return;

            entity.Date = window.Date;
            entity.StartTime = window.StartTime;
            entity.EndTime = window.EndTime;
            entity.ServiceId = window.ServiceId;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAvailabilityWindowAsync(Guid specialistId, Guid windowId)
    {
        var entity = await _context.CalendarAvailabilityWindows
            .FirstOrDefaultAsync(x => x.Id == windowId);

        if (entity == null)
            return;

        _context.CalendarAvailabilityWindows.Remove(entity);

        await _context.SaveChangesAsync();
    }

    public async Task SaveDayOverrideAsync(Guid specialistId, CalendarDayOverride dayOverride)
    {
        var calendar = await GetOrCreateCalendarAsync(specialistId);

        if (dayOverride.Id == Guid.Empty)
        {
            var entity = new CalendarDayOverrideEntity
            {
                Id = Guid.NewGuid(),
                CalendarId = calendar.Id,
                Date = dayOverride.Date,
                Status = dayOverride.Status
            };

            await _context.CalendarDayOverrides.AddAsync(entity);
        }
        else
        {
            var entity = await _context.CalendarDayOverrides
                .FirstOrDefaultAsync(x => x.Id == dayOverride.Id);

            if (entity == null)
                return;

            entity.Date = dayOverride.Date;
            entity.Status = dayOverride.Status;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteDayOverrideAsync(Guid specialistId, Guid overrideId)
    {
        var entity = await _context.CalendarDayOverrides
            .FirstOrDefaultAsync(x => x.Id == overrideId);

        if (entity == null)
            return;

        _context.CalendarDayOverrides.Remove(entity);

        await _context.SaveChangesAsync();
    }

    private async Task<CalendarEntity> GetOrCreateCalendarAsync(Guid specialistId)
    {
        var calendar = await _context.Calendars
            .FirstOrDefaultAsync(x => x.SpecialistId == specialistId);

        if (calendar != null)
            return calendar;

        calendar = new CalendarEntity
        {
            Id = Guid.NewGuid(),
            SpecialistId = specialistId
        };

        await _context.Calendars.AddAsync(calendar);

        await _context.SaveChangesAsync();

        return calendar;
    }

    public async Task<bool> AvailabilityWindowBelongsToSpecialistAsync(Guid specialistId, Guid windowId)
    {
        return await _context.CalendarAvailabilityWindows
            .AnyAsync(x =>
                x.Id == windowId &&
                x.Calendar.SpecialistId == specialistId);
    }

    public async Task<bool> DayOverrideBelongsToSpecialistAsync(Guid specialistId, Guid overrideId)
    {
        return await _context.CalendarDayOverrides
            .AnyAsync(x =>
                x.Id == overrideId &&
                x.Calendar.SpecialistId == specialistId);
    }

    public async Task<bool> AvailabilityWindowExistsAsync(Guid windowId)
    {
        return await _context.CalendarAvailabilityWindows
            .AnyAsync(x => x.Id == windowId);
    }

    public async Task<bool> DayOverrideExistsAsync(Guid overrideId)
    {
        return await _context.CalendarDayOverrides
            .AnyAsync(x => x.Id == overrideId);
    }

    public async Task<bool> HasAvailabilityIntersectionAsync(Guid specialistId, CalendarAvailabilityWindow window)
    {
        return await _context.CalendarAvailabilityWindows
            .AnyAsync(x =>
                x.Calendar.SpecialistId == specialistId
                &&
                x.Date == window.Date
                &&
                x.Id != window.Id
                &&
                window.StartTime < x.EndTime
                &&
                window.EndTime > x.StartTime);
    }

    public async Task<bool> DayOverrideExistsForDateAsync(Guid specialistId, DateOnly date, Guid? excludeId = null)
    {
        return await _context.CalendarDayOverrides
            .AnyAsync(x =>
                x.Calendar.SpecialistId == specialistId
                &&
                x.Date == date
                &&
                (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    public async Task CreateAsync(Guid specialistId)
    {
        var entity = new CalendarEntity
        {
            Id = Guid.NewGuid(),
            SpecialistId = specialistId,
            AvailabilityWindows = [],
            DayOverrides = [],
            BookedSlots = []
        };

        await _context.Calendars.AddAsync(entity);

        await _context.SaveChangesAsync();
    }

    public async Task CreateBookedSlotAsync(Guid specialistId, CalendarBookedSlot slot)
    {
        var calendar = await GetOrCreateCalendarAsync(specialistId);

        var entity = new CalendarBookedSlotEntity
        {
            Id = Guid.NewGuid(),
            CalendarId = calendar.Id,
            OrderId = slot.OrderId,
            ServiceId = slot.ServiceId,
            Date = slot.Date,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime
        };

        await _context.CalendarBookedSlots.AddAsync(entity);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteBookedSlotByOrderIdAsync(Guid orderId)
    {
        var slot = await _context.CalendarBookedSlots
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (slot == null)
            return false;

        _context.CalendarBookedSlots.Remove(slot);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task DeleteAllAvailabilityWindowsForDateAsync(Guid specialistId, DateOnly date)
    {
        var windowsToDelete = await _context.CalendarAvailabilityWindows
            .Where(x => x.Calendar.SpecialistId == specialistId && x.Date == date)
            .ToListAsync();

        if (windowsToDelete.Any())
        {
            _context.CalendarAvailabilityWindows.RemoveRange(windowsToDelete);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearDayAsync(Guid specialistId, DateOnly date)
    {
        var availabilityWindows = await _context.CalendarAvailabilityWindows
            .Where(x =>
                x.Calendar.SpecialistId == specialistId &&
                x.Date == date)
            .ToListAsync();

        if (availabilityWindows.Any())
        {
            _context.CalendarAvailabilityWindows.RemoveRange(availabilityWindows);
        }

        var dayOverrides = await _context.CalendarDayOverrides
            .Where(x =>
                x.Calendar.SpecialistId == specialistId &&
                x.Date == date)
            .ToListAsync();

        if (dayOverrides.Any())
        {
            _context.CalendarDayOverrides.RemoveRange(dayOverrides);
        }

        await _context.SaveChangesAsync();
    }
}
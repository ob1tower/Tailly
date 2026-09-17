using CSharpFunctionalExtensions;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Service.Interfaces;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Calendars;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Application.Service;

public class CalendarService : ICalendarService
{
    private readonly ICalendarRepository _repository;
    private readonly ISpecialistRepository _specialistRepository;
    private readonly ILogger<CalendarService> _logger;

    public CalendarService(ICalendarRepository repository,
                           ISpecialistRepository specialistRepository,
                           ILogger<CalendarService> logger)
    {
        _repository = repository;
        _specialistRepository = specialistRepository;
        _logger = logger;
    }

    public async Task<Result> SaveAvailabilityWindowAsync(string slug, Guid specialistId, CalendarAvailabilityWindow window)
    {
        var specialistBySlug = await _specialistRepository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        var calendar = await _repository.GetAsync(specialistId);
        if (calendar == null)
            return Result.Failure(SpecialistErrors.CalendarNotFound.Description);

        var specialist = await _specialistRepository.GetByIdAsync(specialistId);
        if (specialist == null)
            return Result.Failure(SpecialistErrors.SpecialistNotFound.Description);

        if (window.ServiceId.HasValue && !specialist.Services.Any(x => x.Id == window.ServiceId.Value))
            return Result.Failure(SpecialistErrors.ServiceNotFound.Description);

        if (window.StartTime >= window.EndTime)
            return Result.Failure(SpecialistErrors.InvalidTimeRange.Description);

        if (window.Date < DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Failure(SpecialistErrors.InvalidDate.Description);

        var blockingOverride = calendar.DayOverrides.FirstOrDefault(x =>
            x.Date == window.Date &&
            (x.Status == CalendarDayStatus.DayOff || x.Status == CalendarDayStatus.FullyBooked));

        if (blockingOverride != null)
            return Result.Failure(SpecialistErrors.DayOverrideConflict.Description);

        if (window.Id != Guid.Empty)
        {
            if (!await _repository.AvailabilityWindowExistsAsync(window.Id))
                return Result.Failure(SpecialistErrors.AvailabilityWindowNotFound.Description);

            if (!await _repository.AvailabilityWindowBelongsToSpecialistAsync(specialistId, window.Id))
                return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        if (await _repository.HasAvailabilityIntersectionAsync(specialistId, window))
            return Result.Failure(SpecialistErrors.AvailabilityWindowIntersection.Description);

        await _repository.SaveAvailabilityWindowAsync(specialistId, window);

        _logger.LogInformation("Availability window saved for specialist {SpecialistId}", specialistId);
        return Result.Success();
    }

    public async Task<Result> DeleteAvailabilityWindowAsync(string slug, Guid specialistId, Guid windowId)
    {
        var specialistBySlug = await _specialistRepository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        if (!await _repository.AvailabilityWindowExistsAsync(windowId))
            return Result.Failure(SpecialistErrors.AvailabilityWindowNotFound.Description);

        if (!await _repository.AvailabilityWindowBelongsToSpecialistAsync(specialistId, windowId))
            return Result.Failure(SpecialistErrors.Forbidden.Description);

        await _repository.DeleteAvailabilityWindowAsync(specialistId, windowId);

        _logger.LogInformation("Availability window deleted {WindowId}", windowId);
        return Result.Success();
    }

    public async Task<Result> SaveDayOverrideAsync(string slug, Guid specialistId, CalendarDayOverride dayOverride)
    {
        var specialistBySlug = await _specialistRepository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        var calendar = await _repository.GetAsync(specialistId);
        if (calendar == null)
            return Result.Failure(SpecialistErrors.CalendarNotFound.Description);

        if (dayOverride.Id != Guid.Empty)
        {
            if (!await _repository.DayOverrideExistsAsync(dayOverride.Id))
                return Result.Failure(SpecialistErrors.DayOverrideNotFound.Description);

            if (!await _repository.DayOverrideBelongsToSpecialistAsync(specialistId, dayOverride.Id))
                return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        if (await _repository.DayOverrideExistsForDateAsync(specialistId, dayOverride.Date, dayOverride.Id))
            return Result.Failure(SpecialistErrors.DayOverrideAlreadyExists.Description);

        if (dayOverride.Status == CalendarDayStatus.DayOff ||
            dayOverride.Status == CalendarDayStatus.FullyBooked)
        {
            await _repository.DeleteAllAvailabilityWindowsForDateAsync(specialistId, dayOverride.Date);
        }

        await _repository.SaveDayOverrideAsync(specialistId, dayOverride);

        _logger.LogInformation("Day override saved for specialist {SpecialistId}", specialistId);
        return Result.Success();
    }

    public async Task<Result> DeleteDayOverrideAsync(string slug, Guid specialistId, Guid overrideId)
    {
        var specialistBySlug = await _specialistRepository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        if (!await _repository.DayOverrideExistsAsync(overrideId))
            return Result.Failure(SpecialistErrors.DayOverrideNotFound.Description);

        if (!await _repository.DayOverrideBelongsToSpecialistAsync(specialistId, overrideId))
            return Result.Failure(SpecialistErrors.Forbidden.Description);

        await _repository.DeleteDayOverrideAsync(specialistId, overrideId);

        _logger.LogInformation("Day override deleted {OverrideId}", overrideId);
        return Result.Success();
    }

    public async Task<Result> CreateAsync(string slug, Guid specialistId)
    {
        var specialist = await _specialistRepository.GetBySlugAsync(slug);
        if (specialist == null || specialist.Id != specialistId)
            return Result.Failure(SpecialistErrors.Forbidden.Description);

        if (await _repository.GetAsync(specialistId) != null)
            return Result.Failure(SpecialistErrors.CalendarAlreadyExists.Description);

        await _repository.CreateAsync(specialistId);

        _logger.LogInformation("Calendar created for specialist {SpecialistId}", specialistId);
        return Result.Success();
    }

    public async Task<Result> CreateBookedSlotAsync(Guid specialistId, Guid orderId, Guid serviceId, DateTime startAt, DateTime endAt)
    {
        var calendar = await _repository.GetAsync(specialistId);
        if (calendar == null)
            return Result.Failure(SpecialistErrors.CalendarNotFound.Description);

        var date = DateOnly.FromDateTime(startAt);
        var startTime = TimeOnly.FromDateTime(startAt);
        var endTime = TimeOnly.FromDateTime(endAt);

        var dayOverride = calendar.DayOverrides.FirstOrDefault(x => x.Date == date);
        if (dayOverride != null &&
            (dayOverride.Status == CalendarDayStatus.DayOff || dayOverride.Status == CalendarDayStatus.FullyBooked))
        {
            return Result.Failure(SpecialistErrors.DayOverrideConflict.Description);
        }

        var hasAvailableWindow = calendar.AvailabilityWindows.Any(x =>
            x.Date == date &&
            (x.ServiceId == null || x.ServiceId == serviceId) &&
            startTime >= x.StartTime &&
            endTime <= x.EndTime);

        if (!hasAvailableWindow)
        {
            return Result.Failure(SpecialistErrors.AvailabilityWindowNotFound.Description);
        }

        var hasBookedConflict = calendar.BookedSlots.Any(x =>
            x.Date == date &&
            startTime < x.EndTime &&
            endTime > x.StartTime);

        if (hasBookedConflict)
            return Result.Failure(SpecialistErrors.BookedSlotIntersection.Description);

        var bookedSlot = new CalendarBookedSlot
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ServiceId = serviceId,
            Date = date,
            StartTime = startTime,
            EndTime = endTime
        };

        await _repository.CreateBookedSlotAsync(specialistId, bookedSlot);

        return Result.Success();
    }

    public async Task<Result> DeleteBookedSlotByOrderIdAsync(Guid orderId)
    {
        var result = await _repository.DeleteBookedSlotByOrderIdAsync(orderId);
        return result
            ? Result.Success()
            : Result.Failure(SpecialistErrors.BookedSlotNotFound.Description);
    }

    public async Task<Result> ClearDayAsync(string slug, Guid specialistId, string date)
    {
        var specialistBySlug = await _specialistRepository.GetBySlugAsync(slug);
        if (specialistBySlug == null || specialistBySlug.Id != specialistId)
        {
            _logger.LogWarning("Slug does not match specialist. Slug: {Slug}, SpecialistId: {SpecialistId}", slug, specialistId);
            return Result.Failure(SpecialistErrors.Forbidden.Description);
        }

        var parsed = DateOnly.TryParse(date, out var parsedDate);

        if (!parsed)
            return Result.Failure(SpecialistErrors.InvalidDate.Description);

        var calendar = await _repository.GetAsync(specialistId);

        if (calendar == null)
            return Result.Failure(SpecialistErrors.CalendarNotFound.Description);

        if (parsedDate < DateOnly.FromDateTime(DateTime.UtcNow))
            return Result.Failure(SpecialistErrors.InvalidDate.Description);

        var hasBookedSlots = calendar.BookedSlots.Any(x => x.Date == parsedDate);

        if (hasBookedSlots)
            return Result.Failure(SpecialistErrors.CannotClearBookedDay.Description);

        var hasAvailabilityWindows = calendar.AvailabilityWindows
            .Any(x => x.Date == parsedDate);

        var hasDayOverrides = calendar.DayOverrides
            .Any(x => x.Date == parsedDate);

        if (!hasAvailabilityWindows && !hasDayOverrides)
            return Result.Failure(SpecialistErrors.CalendarDayIsEmpty.Description);

        await _repository.ClearDayAsync(specialistId, parsedDate);

        _logger.LogInformation(
            "Calendar day cleared for specialist {SpecialistId} date {Date}",
            specialistId,
            date);

        return Result.Success();
    }
}
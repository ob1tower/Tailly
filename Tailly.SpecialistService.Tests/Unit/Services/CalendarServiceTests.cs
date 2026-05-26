using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tailly.SpecialistService.Application.Errors;
using Tailly.SpecialistService.Application.Service;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Calendars;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Tests.Unit.Services;

/// <summary>
/// Unit tests for CalendarService.
/// Covers ALL methods with positive and negative scenarios:
/// - SaveAvailabilityWindowAsync (success + forbidden slug + calendar not found + specialist not found + service not found + invalid time range + past date + day override conflict + window not found + forbidden window + intersection)
/// - DeleteAvailabilityWindowAsync (success + forbidden slug + window not found)
/// - SaveDayOverrideAsync (success)
/// - DeleteDayOverrideAsync (success)
/// - CreateAsync (success + calendar already exists)
/// - CreateBookedSlotAsync (success)
/// - DeleteBookedSlotByOrderIdAsync (success + booked slot not found)
/// - ClearDayAsync (success + forbidden slug + invalid date)
/// </summary>
public class CalendarServiceTests
{
    private readonly Mock<ICalendarRepository> _repository = new();
    private readonly Mock<ISpecialistRepository> _specialistRepository = new();
    private readonly Mock<ILogger<CalendarService>> _logger = new();

    private CalendarService CreateService() =>
        new CalendarService(_repository.Object, _specialistRepository.Object, _logger.Object);

    private CalendarAvailabilityWindow CreateValidAvailabilityWindow() => new()
    {
        Id = Guid.NewGuid(),
        Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
        StartTime = new TimeOnly(10, 0),
        EndTime = new TimeOnly(11, 0)
    };

    private CalendarDayOverride CreateValidDayOverride() => new()
    {
        Id = Guid.NewGuid(),
        Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
        Status = CalendarDayStatus.Available
    };

    // =============================================
    // ============== SaveAvailabilityWindowAsync ==============
    // =============================================

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Save_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var window = CreateValidAvailabilityWindow();
        window.Id = Guid.Empty;

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar());
        _specialistRepository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(new Specialist());
        _repository.Setup(x => x.HasAvailabilityIntersectionAsync(specialistId, window)).ReturnsAsync(false);
        _repository.Setup(x => x.SaveAvailabilityWindowAsync(specialistId, window)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.SaveAvailabilityWindowAsync(slug, specialistId, window);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Return_Forbidden_When_Slug_Does_Not_Match_Specialist()
    {
        // arrange
        var service = CreateService();
        _specialistRepository.Setup(x => x.GetBySlugAsync("wrong-slug")).ReturnsAsync((Specialist?)null);

        // act
        var result = await service.SaveAvailabilityWindowAsync("wrong-slug", Guid.NewGuid(), CreateValidAvailabilityWindow());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.Forbidden.Description);
    }

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Return_CalendarNotFound()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync((Calendar?)null);

        var service = CreateService();

        // act
        var result = await service.SaveAvailabilityWindowAsync(slug, specialistId, CreateValidAvailabilityWindow());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.CalendarNotFound.Description);
    }

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Return_SpecialistNotFound()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar());
        _specialistRepository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync((Specialist?)null);

        var service = CreateService();

        // act
        var result = await service.SaveAvailabilityWindowAsync(slug, specialistId, CreateValidAvailabilityWindow());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.SpecialistNotFound.Description);
    }

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Return_ServiceNotFound_When_ServiceId_Not_Belongs_To_Specialist()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var window = CreateValidAvailabilityWindow();
        window.ServiceId = Guid.NewGuid();

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar());
        _specialistRepository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(new Specialist());

        var service = CreateService();

        // act
        var result = await service.SaveAvailabilityWindowAsync(slug, specialistId, window);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.ServiceNotFound.Description);
    }

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Return_InvalidTimeRange()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var window = CreateValidAvailabilityWindow();
        window.Id = Guid.Empty;
        window.StartTime = new TimeOnly(11, 0);
        window.EndTime = new TimeOnly(10, 0);

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar());
        _specialistRepository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(new Specialist());

        var service = CreateService();

        // act
        var result = await service.SaveAvailabilityWindowAsync(slug, specialistId, window);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.InvalidTimeRange.Description);
    }

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Return_InvalidDate_When_Past_Date()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var window = CreateValidAvailabilityWindow();
        window.Id = Guid.Empty;
        window.Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar());
        _specialistRepository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(new Specialist());

        var service = CreateService();

        // act
        var result = await service.SaveAvailabilityWindowAsync(slug, specialistId, window);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.InvalidDate.Description);
    }

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Return_DayOverrideConflict()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var window = CreateValidAvailabilityWindow();
        window.Id = Guid.Empty;

        var calendar = new Calendar
        {
            DayOverrides =
            [
                new CalendarDayOverride
                {
                    Date = window.Date,
                    Status = CalendarDayStatus.DayOff
                }
            ]
        };

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(calendar);
        _specialistRepository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(new Specialist());

        var service = CreateService();

        // act
        var result = await service.SaveAvailabilityWindowAsync(slug, specialistId, window);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.DayOverrideConflict.Description);
    }

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Return_AvailabilityWindowNotFound_When_Updating_NonExisting_Window()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var window = CreateValidAvailabilityWindow();
        window.Id = Guid.NewGuid();

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar());
        _specialistRepository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(new Specialist());
        _repository.Setup(x => x.AvailabilityWindowExistsAsync(window.Id)).ReturnsAsync(false);

        var service = CreateService();

        // act
        var result = await service.SaveAvailabilityWindowAsync(slug, specialistId, window);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.AvailabilityWindowNotFound.Description);
    }

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Return_Forbidden_When_Window_Does_Not_Belong_To_Specialist()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var window = CreateValidAvailabilityWindow();
        window.Id = Guid.NewGuid();

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar());
        _specialistRepository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(new Specialist());
        _repository.Setup(x => x.AvailabilityWindowExistsAsync(window.Id)).ReturnsAsync(true);
        _repository.Setup(x => x.AvailabilityWindowBelongsToSpecialistAsync(specialistId, window.Id)).ReturnsAsync(false);

        var service = CreateService();

        // act
        var result = await service.SaveAvailabilityWindowAsync(slug, specialistId, window);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.Forbidden.Description);
    }

    [Fact]
    public async Task SaveAvailabilityWindowAsync_Should_Return_AvailabilityWindowIntersection()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var window = CreateValidAvailabilityWindow();
        window.Id = Guid.Empty;

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar());
        _specialistRepository.Setup(x => x.GetByIdAsync(specialistId)).ReturnsAsync(new Specialist());
        _repository.Setup(x => x.HasAvailabilityIntersectionAsync(specialistId, window)).ReturnsAsync(true);

        var service = CreateService();

        // act
        var result = await service.SaveAvailabilityWindowAsync(slug, specialistId, window);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.AvailabilityWindowIntersection.Description);
    }

    // =============================================
    // ============== DeleteAvailabilityWindowAsync ==============
    // =============================================

    [Fact]
    public async Task DeleteAvailabilityWindowAsync_Should_Delete_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var windowId = Guid.NewGuid();

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.AvailabilityWindowExistsAsync(windowId)).ReturnsAsync(true);
        _repository.Setup(x => x.AvailabilityWindowBelongsToSpecialistAsync(specialistId, windowId)).ReturnsAsync(true);
        _repository.Setup(x => x.DeleteAvailabilityWindowAsync(specialistId, windowId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.DeleteAvailabilityWindowAsync(slug, specialistId, windowId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAvailabilityWindowAsync_Should_Return_Forbidden_When_Slug_Does_Not_Match()
    {
        // arrange
        var service = CreateService();
        _specialistRepository.Setup(x => x.GetBySlugAsync("wrong")).ReturnsAsync((Specialist?)null);

        // act
        var result = await service.DeleteAvailabilityWindowAsync("wrong", Guid.NewGuid(), Guid.NewGuid());

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.Forbidden.Description);
    }

    [Fact]
    public async Task DeleteAvailabilityWindowAsync_Should_Return_AvailabilityWindowNotFound()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var windowId = Guid.NewGuid();

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.AvailabilityWindowExistsAsync(windowId)).ReturnsAsync(false);

        var service = CreateService();

        // act
        var result = await service.DeleteAvailabilityWindowAsync(slug, specialistId, windowId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.AvailabilityWindowNotFound.Description);
    }

    // =============================================
    // ============== SaveDayOverrideAsync ==============
    // =============================================

    [Fact]
    public async Task SaveDayOverrideAsync_Should_Save_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var dayOverride = CreateValidDayOverride();
        dayOverride.Id = Guid.Empty;

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar());
        _repository.Setup(x => x.SaveDayOverrideAsync(specialistId, dayOverride)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.SaveDayOverrideAsync(slug, specialistId, dayOverride);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== DeleteDayOverrideAsync ==============
    // =============================================

    [Fact]
    public async Task DeleteDayOverrideAsync_Should_Delete_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var overrideId = Guid.NewGuid();

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.DayOverrideExistsAsync(overrideId)).ReturnsAsync(true);
        _repository.Setup(x => x.DayOverrideBelongsToSpecialistAsync(specialistId, overrideId)).ReturnsAsync(true);
        _repository.Setup(x => x.DeleteDayOverrideAsync(specialistId, overrideId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.DeleteDayOverrideAsync(slug, specialistId, overrideId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== CreateAsync ==============
    // =============================================

    [Fact]
    public async Task CreateAsync_Should_Create_Calendar_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync((Calendar?)null);
        _repository.Setup(x => x.CreateAsync(specialistId)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CreateAsync(slug, specialistId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_Should_Return_CalendarAlreadyExists()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar());

        var service = CreateService();

        // act
        var result = await service.CreateAsync(slug, specialistId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.CalendarAlreadyExists.Description);
    }

    // =============================================
    // ============== CreateBookedSlotAsync ==============
    // =============================================

    [Fact]
    public async Task CreateBookedSlotAsync_Should_Create_Booked_Slot_Successfully()
    {
        // arrange
        var specialistId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddHours(3);
        var end = start.AddHours(1);

        var calendar = new Calendar
        {
            AvailabilityWindows = new List<CalendarAvailabilityWindow>
            {
                new() { Date = DateOnly.FromDateTime(start), StartTime = TimeOnly.FromDateTime(start), EndTime = TimeOnly.FromDateTime(end) }
            }
        };

        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(calendar);
        _repository.Setup(x => x.CreateBookedSlotAsync(specialistId, It.IsAny<CalendarBookedSlot>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CreateBookedSlotAsync(specialistId, orderId, serviceId, start, end);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== DeleteBookedSlotByOrderIdAsync ==============
    // =============================================

    [Fact]
    public async Task DeleteBookedSlotByOrderIdAsync_Should_Delete_Successfully()
    {
        // arrange
        var orderId = Guid.NewGuid();
        _repository.Setup(x => x.DeleteBookedSlotByOrderIdAsync(orderId)).ReturnsAsync(true);

        var service = CreateService();

        // act
        var result = await service.DeleteBookedSlotByOrderIdAsync(orderId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteBookedSlotByOrderIdAsync_Should_Return_BookedSlotNotFound()
    {
        // arrange
        var orderId = Guid.NewGuid();
        _repository.Setup(x => x.DeleteBookedSlotByOrderIdAsync(orderId)).ReturnsAsync(false);

        var service = CreateService();

        // act
        var result = await service.DeleteBookedSlotByOrderIdAsync(orderId);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.BookedSlotNotFound.Description);
    }

    // =============================================
    // ============== ClearDayAsync ==============
    // =============================================

    [Fact]
    public async Task ClearDayAsync_Should_Clear_Day_Successfully()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        var dateStr = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)).ToString("yyyy-MM-dd");

        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });
        _repository.Setup(x => x.GetAsync(specialistId)).ReturnsAsync(new Calendar
        {
            AvailabilityWindows =
            [
                new CalendarAvailabilityWindow
                {
                    Date = DateOnly.Parse(dateStr),
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(11, 0)
                }
            ]
        });
        _repository.Setup(x => x.ClearDayAsync(specialistId, It.IsAny<DateOnly>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.ClearDayAsync(slug, specialistId, dateStr);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ClearDayAsync_Should_Return_Forbidden_When_Slug_Does_Not_Match()
    {
        // arrange
        var service = CreateService();
        _specialistRepository.Setup(x => x.GetBySlugAsync("wrong")).ReturnsAsync((Specialist?)null);

        // act
        var result = await service.ClearDayAsync("wrong", Guid.NewGuid(), "2026-05-25");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.Forbidden.Description);
    }

    [Fact]
    public async Task ClearDayAsync_Should_Return_InvalidDate()
    {
        // arrange
        var slug = "ivan-ivanov";
        var specialistId = Guid.NewGuid();
        _specialistRepository.Setup(x => x.GetBySlugAsync(slug)).ReturnsAsync(new Specialist { Id = specialistId });

        var service = CreateService();

        // act
        var result = await service.ClearDayAsync(slug, specialistId, "неправильная-дата");

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SpecialistErrors.InvalidDate.Description);
    }
}
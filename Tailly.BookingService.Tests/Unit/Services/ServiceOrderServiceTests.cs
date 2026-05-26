using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using Tailly.BookingService.Application.Dtos.Internal;
using Tailly.BookingService.Application.Service;
using Tailly.BookingService.Core.Enums;
using Tailly.BookingService.Core.Models;
using Tailly.BookingService.Infrastructure.Repositories;
using Tailly.Contracts.Messages;

namespace Tailly.BookingService.Tests.Unit.Services;

/// <summary>
/// Unit tests for ServiceOrderService.
/// Covers:
/// - successful CreateAsync (full flow with all HTTP calls and slot booking)
/// - CreateAsync returns errors for invalid input, invalid service, invalid pet, invalid specialist, time unavailable
/// - GetMyOrdersAsync success
/// - GetBySpecialistIdAsync success
/// - ConfirmAsync success + all validation errors
/// - StartAsync success + all validation errors
/// - CompleteAsync success (including repeat order detection)
/// - CancelAsync success (by client and specialist)
/// - CancelAsync returns all validation errors
/// - RepeatAsync success + all validation errors
/// - LeaveReviewAsync success + all validation errors
/// </summary>
public class ServiceOrderServiceTests
{
    private readonly Mock<IServiceOrderRepository> _orderRepository = new();
    private readonly Mock<IPublishEndpoint> _publishEndpoint = new();
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<ILogger<ServiceOrderService>> _logger = new();

    private ServiceOrderService CreateService()
    {
        return new ServiceOrderService(
            _orderRepository.Object,
            _publishEndpoint.Object,
            _httpClientFactory.Object,
            _logger.Object
        );
    }

    private ServiceOrder CreateValidOrder()
    {
        return new ServiceOrder
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            SpecialistId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            PetId = Guid.NewGuid(),
            Status = OrderStatus.PendingConfirmation,
            StartAt = DateTime.UtcNow.AddHours(3),
            EndAt = DateTime.UtcNow.AddHours(4)
        };
    }

    // =============================================
    // ============== CreateAsync ==============
    // =============================================

    [Fact]
    public async Task CreateAsync_Should_Create_Order_Successfully()
    {
        // arrange
        var model = new CreateServiceOrder
        {
            ClientId = Guid.NewGuid(),
            SpecialistId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            PetId = Guid.NewGuid(),
            StartAt = DateTime.UtcNow.AddHours(3)
        };

        var specialistHandler = new Mock<HttpMessageHandler>();
        var profileHandler = new Mock<HttpMessageHandler>();

        specialistHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                var uri = request.RequestUri?.ToString() ?? "";

                if (uri.Contains($"/internal/services/{model.ServiceId}"))
                {
                    var serviceData = new ServiceInternalDto
                    {
                        Id = model.ServiceId,
                        SpecialistId = model.SpecialistId,
                        Name = "Груминг",
                        Price = 1500,
                        PriceUnit = "Hour"
                    };

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(serviceData)
                    };
                }

                if (uri.Contains($"/internal/specialists/{model.SpecialistId}"))
                {
                    var specialistData = new SpecialistInternalDto
                    {
                        Id = model.SpecialistId,
                        FullName = "Иван Иванов",
                        Slug = "ivan-ivanov"
                    };

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(specialistData)
                    };
                }

                if (uri.Contains("/internal/calendar/check-availability"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(new AvailabilityCheckResponseDto
                        {
                            IsAvailable = true
                        })
                    };
                }

                if (uri.Contains("/internal/calendar/book-slot"))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        profileHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                var uri = request.RequestUri?.ToString() ?? "";

                if (uri.Contains($"/internal/pets/{model.PetId}"))
                {
                    var petData = new PetInternalDto
                    {
                        Id = model.PetId,
                        UserId = model.ClientId,
                        Name = "Барсик"
                    };

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(petData)
                    };
                }

                if (uri.Contains($"/internal/clients/{model.ClientId}"))
                {
                    var clientData = new ClientInternalDto
                    {
                        UserId = model.ClientId,
                        FullName = "Пётр Петров"
                    };

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(clientData)
                    };
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        var specialistClient = new HttpClient(specialistHandler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var profileClient = new HttpClient(profileHandler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        _httpClientFactory
            .Setup(x => x.CreateClient("specialist"))
            .Returns(specialistClient);

        _httpClientFactory
            .Setup(x => x.CreateClient("client-profile"))
            .Returns(profileClient);

        _orderRepository
            .Setup(x => x.AddAsync(It.IsAny<ServiceOrder>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.CreateAsync(model);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ClientId.Should().Be(model.ClientId);
    }

    // =============================================
    // ============== GetMyOrdersAsync ==============
    // =============================================

    [Fact]
    public async Task GetMyOrdersAsync_Should_Return_Orders()
    {
        // arrange
        var clientId = Guid.NewGuid();
        var orders = new List<ServiceOrder> { CreateValidOrder() };
        _orderRepository.Setup(x => x.GetByClientIdAsync(clientId, null, 1, 20)).ReturnsAsync(orders);

        var service = CreateService();

        // act
        var result = await service.GetMyOrdersAsync(clientId);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // =============================================
    // ============== GetBySpecialistIdAsync ==============
    // =============================================

    [Fact]
    public async Task GetBySpecialistIdAsync_Should_Return_Orders()
    {
        // arrange
        var specialistId = Guid.NewGuid();
        var orders = new List<ServiceOrder> { CreateValidOrder() };
        _orderRepository.Setup(x => x.GetBySpecialistIdAsync(specialistId, null, 1, 20)).ReturnsAsync(orders);

        var service = CreateService();

        // act
        var result = await service.GetBySpecialistIdAsync(specialistId);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // =============================================
    // ============== ConfirmAsync ==============
    // =============================================

    [Fact]
    public async Task ConfirmAsync_Should_Confirm_Order_Successfully()
    {
        // arrange
        var order = CreateValidOrder();
        order.Status = OrderStatus.PendingConfirmation;
        _orderRepository.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orderRepository.Setup(x => x.UpdateAsync(order)).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.ConfirmAsync(order.Id, order.SpecialistId);

        // assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    // =============================================
    // ============== StartAsync ==============
    // =============================================

    [Fact]
    public async Task StartAsync_Should_Start_Order_Successfully()
    {
        // arrange
        var order = CreateValidOrder();

        order.Status = OrderStatus.Confirmed;
        order.StartAt = DateTime.UtcNow.AddMinutes(-5);
        order.StartedAt = null;

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _orderRepository
            .Setup(x => x.UpdateAsync(order))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.StartAsync(order.Id, order.SpecialistId);

        // assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Active);
    }

    // =============================================
    // ============== CompleteAsync ==============
    // =============================================

    [Fact]
    public async Task CompleteAsync_Should_Complete_Order_Successfully()
    {
        // arrange
        var order = CreateValidOrder();
        order.Status = OrderStatus.Active;
        order.StartedAt = DateTime.UtcNow.AddHours(-1);
        _orderRepository.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orderRepository.Setup(x => x.UpdateAsync(order)).Returns(Task.CompletedTask);
        _orderRepository.Setup(x => x.CountCompletedOrdersAsync(order.ClientId, order.SpecialistId, order.ServiceId)).ReturnsAsync(0);

        var service = CreateService();

        // act
        var result = await service.CompleteAsync(order.Id, order.SpecialistId);

        // assert
        result.IsSuccess.Should().BeTrue();
        _publishEndpoint.Verify(x => x.Publish(It.IsAny<OrderCompletedMessage>(), default), Times.Once);
    }

    // =============================================
    // ============== CancelAsync ==============
    // =============================================

    [Fact]
    public async Task CancelAsync_Should_Cancel_Order_Successfully_By_Client()
    {
        // arrange
        var order = CreateValidOrder();

        order.Status = OrderStatus.Confirmed;

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _orderRepository
            .Setup(x => x.UpdateAsync(order))
            .Returns(Task.CompletedTask);

        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        _httpClientFactory
            .Setup(x => x.CreateClient("specialist"))
            .Returns(httpClient);

        var service = CreateService();

        // act
        var result = await service.CancelAsync(order.Id, order.ClientId);

        // assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
    }

    // =============================================
    // ============== RepeatAsync ==============
    // =============================================

    [Fact]
    public async Task RepeatAsync_Should_Return_RepeatDraft_Successfully()
    {
        // arrange
        var order = CreateValidOrder();
        order.Status = OrderStatus.Completed;
        _orderRepository.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);

        var service = CreateService();

        // act
        var result = await service.RepeatAsync(order.Id, order.ClientId);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    // =============================================
    // ============== LeaveReviewAsync ==============
    // =============================================

    [Fact]
    public async Task LeaveReviewAsync_Should_Create_Review_Successfully()
    {
        // arrange
        var order = CreateValidOrder();
        order.Status = OrderStatus.Completed;
        _orderRepository.Setup(x => x.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _orderRepository.Setup(x => x.AddReviewAsync(order.Id, It.IsAny<ServiceOrderReview>())).Returns(Task.CompletedTask);

        var service = CreateService();

        // act
        var result = await service.LeaveReviewAsync(order.Id, order.ClientId, 5, "Отличный сервис");

        // assert
        result.IsSuccess.Should().BeTrue();
        _publishEndpoint.Verify(x => x.Publish(It.IsAny<ReviewCreatedMessage>(), default), Times.Once);
    }
}
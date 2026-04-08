using Application.Common.Interfaces.Persistence;
using Application.Orders.Commands.CreateOrder;
using Domain.Aggregates.Event;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using EventId = Domain.ValueObjects.Identities.EventId;

namespace ApplicationUnitTests.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IEventRepository _eventRepository = Substitute.For<IEventRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<CreateOrderCommandHandler> _logger = Substitute.For<ILogger<CreateOrderCommandHandler>>();

    private CreateOrderCommandHandler CreateSut() => new(_orderRepository, _eventRepository, _unitOfWork, _logger);

    private static (Event Event, TicketCategory Category) CreateEventWithCategory(
        bool isActive = true, int quota = 100, int soldCount = 0)
    {
        var eventId = EventId.NewId();
        var category = TicketCategory.Create(eventId, "VIP", Money.From(5000), quota, isActive: isActive);
        var slug = EventSlug.Create("order-test");
        var venue = Venue.Create("Stadium", "Paris", "123 Main St");
        var start = DateTimeOffset.UtcNow.AddDays(30);
        var end = DateTimeOffset.UtcNow.AddDays(31);
        var salesPeriod = SalesPeriod.Create(start, end);
        var evt = Event.Create(Guid.NewGuid(), "order-test", "Desc", slug, venue, start, end, salesPeriod, new[] { category });
        return (evt, category);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ReturnsFailure()
    {
        // Arrange
        _eventRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Event>());
        var cmd = new CreateOrderCommand(Guid.NewGuid(), 1, "+22670000000", null);

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_CategoryNotActive_ReturnsFailure()
    {
        // Arrange
        var (evt, category) = CreateEventWithCategory(isActive: false);
        _eventRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Event> { evt });
        var cmd = new CreateOrderCommand(category.Id, 1, "+22670000000", null);

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("active", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_InsufficientStock_ReturnsFailure()
    {
        // Arrange – quota is 2, requesting 5
        var (evt, category) = CreateEventWithCategory(quota: 2);
        _eventRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Event> { evt });
        var cmd = new CreateOrderCommand(category.Id, 5, "+22670000000", null);

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("insuffisant", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesOrderAndReturnsSuccess()
    {
        // Arrange
        var (evt, category) = CreateEventWithCategory();
        _eventRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Event> { evt });
        var cmd = new CreateOrderCommand(category.Id, 2, "+22670000000", null);

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Quantity);
        await _orderRepository.Received(1).AddAsync(Arg.Any<Domain.Aggregates.Sales.Order>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

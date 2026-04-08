using Application.Common.Interfaces.Persistence;
using Application.Events.Commands.PatchEvent;
using Domain.Aggregates.Event;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using EventId = Domain.ValueObjects.Identities.EventId;

namespace ApplicationUnitTests.Events;

public class PatchEventCommandHandlerTests
{
    private readonly IEventRepository _eventRepository = Substitute.For<IEventRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ILogger<PatchEventCommandHandler> _logger = Substitute.For<ILogger<PatchEventCommandHandler>>();

    private PatchEventCommandHandler CreateSut() => new(_eventRepository, _unitOfWork, _logger);

    private static Event CreateTestEvent(bool withCategory = false)
    {
        var slug = EventSlug.Create("test-event");
        var venue = Venue.Create("Stadium", "Paris", "123 Main St");
        var start = DateTimeOffset.UtcNow.AddDays(30);
        var end = DateTimeOffset.UtcNow.AddDays(31);
        var salesPeriod = SalesPeriod.Create(start, end);

        var categories = withCategory
            ? new[] { TicketCategory.Create(EventId.NewId(), "VIP", Money.From(100), 50) }
            : Array.Empty<TicketCategory>();

        return Event.Create(Guid.NewGuid(), "test-event", "Desc", slug, venue, start, end, salesPeriod, categories);
    }

    [Fact]
    public async Task Handle_EventNotFound_ReturnsFailure()
    {
        // Arrange
        _eventRepository.GetByIdAsync(Arg.Any<EventId>(), Arg.Any<CancellationToken>())
            .Returns((Event?)null);
        var cmd = new PatchEventCommand(Guid.NewGuid(), "Publish");

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("not found", result.Error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_PublishStatus_PublishesEvent()
    {
        // Arrange
        var evt = CreateTestEvent(withCategory: true);
        _eventRepository.GetByIdAsync(Arg.Any<EventId>(), Arg.Any<CancellationToken>())
            .Returns(evt);
        var cmd = new PatchEventCommand(evt.Id.Value, "Publish");

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Published", result.Value.Status);
        await _eventRepository.Received(1).UpdateAsync(evt);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DraftStatus_UnpublishesEvent()
    {
        // Arrange – create a published event
        var evt = CreateTestEvent(withCategory: true);
        evt.Publish();
        Assert.True(evt.IsPublished);

        _eventRepository.GetByIdAsync(Arg.Any<EventId>(), Arg.Any<CancellationToken>())
            .Returns(evt);
        var cmd = new PatchEventCommand(evt.Id.Value, "Draft");

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Draft", result.Value.Status);
    }
}

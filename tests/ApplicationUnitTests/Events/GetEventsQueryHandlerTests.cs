using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Events.Queries.GetEvents;
using Domain.Aggregates.Event;
using Domain.ValueObjects;
using Domain.ValueObjects.Identities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ApplicationUnitTests.Events;

public class GetEventsQueryHandlerTests
{
    private readonly IEventRepository _eventRepository = Substitute.For<IEventRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ILogger<GetEventsQueryHandler> _logger = Substitute.For<ILogger<GetEventsQueryHandler>>();

    private GetEventsQueryHandler CreateSut() => new(_eventRepository, _currentUserService, _logger);

    private static Event CreateTestEvent(Guid organizerId, string name = "test-event")
    {
        var slug = EventSlug.Create(name);
        var venue = Venue.Create("Stadium", "Paris", "123 Main St");
        var start = DateTimeOffset.UtcNow.AddDays(30);
        var end = DateTimeOffset.UtcNow.AddDays(31);
        var salesPeriod = SalesPeriod.Create(start, end);
        return Event.Create(organizerId, name, "Desc", slug, venue, start, end, salesPeriod, Array.Empty<TicketCategory>());
    }

    [Fact]
    public async Task Handle_NoEvents_ReturnsEmptyPagedResult()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        _currentUserService.UserId.Returns(orgId.ToString());
        _eventRepository.GetByOrganizerIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Event>());

        // Act
        var result = await CreateSut().Handle(new GetEventsQuery(null), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_WithEvents_ReturnsPagedEventDtos()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        _currentUserService.UserId.Returns(orgId.ToString());
        var events = new List<Event>
        {
            CreateTestEvent(orgId, "event-one"),
            CreateTestEvent(orgId, "event-two")
        };
        _eventRepository.GetByOrganizerIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(events);

        // Act
        var result = await CreateSut().Handle(new GetEventsQuery(null, 1, 20), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_FiltersCorrectly()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        _currentUserService.UserId.Returns(orgId.ToString());

        var draftEvent = CreateTestEvent(orgId, "draft-evt");
        // draftEvent is Draft by default, so filtering for "Published" should return 0
        _eventRepository.GetByOrganizerIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Event> { draftEvent });

        // Act
        var result = await CreateSut().Handle(new GetEventsQuery("Published", 1, 20), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }
}

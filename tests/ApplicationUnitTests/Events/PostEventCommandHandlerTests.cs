using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using Application.Events.Commands.PostEvent;
using Domain.ValueObjects.Identities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ApplicationUnitTests.Events;

public class PostEventCommandHandlerTests
{
    private readonly ILogger<PostEventCommandHandler> _logger = Substitute.For<ILogger<PostEventCommandHandler>>();
    private readonly IEventRepository _eventRepository = Substitute.For<IEventRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private PostEventCommandHandler CreateSut() =>
        new(_logger, _eventRepository, _unitOfWork, _currentUserService);

    private static PostEventCommand ValidCommand() => new(
        Name: "test-event",
        Description: "A test event",
        VenueName: "Stadium",
        City: "Paris",
        Address: "123 Main St",
        GpsCoordinates: null,
        StartDate: DateTimeOffset.UtcNow.AddDays(30),
        EndDate: DateTimeOffset.UtcNow.AddDays(31),
        CoverImageUrl: null,
        Capacity: 100);

    [Fact]
    public async Task Handle_ValidCommand_CreatesEventAndReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserService.UserId.Returns(userId.ToString());
        var handler = CreateSut();

        // Act
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("test-event", result.Value.Name);
        Assert.Equal(userId, result.Value.OrganizerId);

        await _eventRepository.Received(1).AddAsync(Arg.Any<Domain.Aggregates.Event.Event>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NullUserId_UsesEmptyGuid()
    {
        // Arrange
        _currentUserService.UserId.Returns((string?)null);
        var handler = CreateSut();

        // Act
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Guid.Empty, result.Value.OrganizerId);
    }
}

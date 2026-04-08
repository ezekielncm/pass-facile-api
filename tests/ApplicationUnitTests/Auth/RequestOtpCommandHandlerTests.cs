using Application.Auth.Commands.RequestOtp;
using Application.Common.Interfaces.Auth;
using NSubstitute;

namespace ApplicationUnitTests.Auth;

public class RequestOtpCommandHandlerTests
{
    private readonly IAuth _auth = Substitute.For<IAuth>();

    private RequestOtpCommandHandler CreateSut() => new(_auth);

    [Fact]
    public async Task Handle_Success_ReturnsOtpDto()
    {
        // Arrange
        var otpId = Guid.NewGuid().ToString();
        var expires = DateTimeOffset.UtcNow.AddMinutes(5);
        _auth.RequestOtpAsync(Arg.Any<string>())
            .Returns((true, otpId, expires, (string?)null));

        var cmd = new RequestOtpCommand("+22670000000");

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(otpId, result.Value.OtpId);
        Assert.Equal(expires, result.Value.ExpiresAt);
    }

    [Fact]
    public async Task Handle_Failure_ReturnsValidationError()
    {
        // Arrange
        _auth.RequestOtpAsync(Arg.Any<string>())
            .Returns((false, (string?)null, (DateTimeOffset?)null, "Rate limit exceeded"));

        var cmd = new RequestOtpCommand("+22670000000");

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Rate limit exceeded", result.Error.Message);
    }
}

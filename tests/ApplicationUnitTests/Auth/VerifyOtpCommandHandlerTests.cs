using Application.Auth.Commands.VerifyOtp;
using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Persistence;
using NSubstitute;

namespace ApplicationUnitTests.Auth;

public class VerifyOtpCommandHandlerTests
{
    private readonly IAuth _auth = Substitute.For<IAuth>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private VerifyOtpCommandHandler CreateSut() => new(_auth, _userRepository, _unitOfWork);

    [Fact]
    public async Task Handle_Success_ReturnsTokens()
    {
        // Arrange
        _auth.VerifyOtpAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns((true, "access-token-123", "refresh-token-456", (string?)null));

        var cmd = new VerifyOtpCommand("+22670000000", "123456", "device-1");

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("access-token-123", result.Value.AccessToken);
        Assert.Equal("refresh-token-456", result.Value.RefreshToken);
    }

    [Fact]
    public async Task Handle_Failure_ReturnsValidationError()
    {
        // Arrange
        _auth.VerifyOtpAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns((false, (string?)null, (string?)null, "Invalid OTP"));

        var cmd = new VerifyOtpCommand("+22670000000", "000000", "device-1");

        // Act
        var result = await CreateSut().Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Invalid OTP", result.Error.Message);
    }
}

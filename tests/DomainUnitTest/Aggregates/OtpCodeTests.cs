using Domain.Aggregates.User;
using Domain.Common;
using Domain.ValueObjects;

namespace DomainUnitTest.Aggregates;

public class OtpCodeTests
{
    private static readonly PhoneNumber ValidPhone = new("226", "70000001");
    private static readonly Guid UserId = Guid.NewGuid();

    private static OTPCode IssueValid(DateTimeOffset? now = null) =>
        OTPCode.Issue(UserId, ValidPhone, "123456", now ?? DateTimeOffset.UtcNow);

    // --- Issue ---

    [Fact]
    public void Issue_SetsPropertiesWithFiveMinuteExpiry()
    {
        var now = DateTimeOffset.UtcNow;

        var otp = OTPCode.Issue(UserId, ValidPhone, "123456", now);

        Assert.Equal(UserId, otp.UserId);
        Assert.Equal(ValidPhone, otp.PhoneNumber);
        Assert.Equal("123456", otp.Code);
        Assert.Equal(now, otp.CreatedAt);
        Assert.Equal(now.AddMinutes(5), otp.ExpiresAt);
        Assert.False(otp.IsUsed);
        Assert.Null(otp.UsedAt);
    }

    [Fact]
    public void Issue_TrimsCode()
    {
        var otp = OTPCode.Issue(UserId, ValidPhone, " 7890 ", DateTimeOffset.UtcNow);

        Assert.Equal("7890", otp.Code);
    }

    // --- IsExpired ---

    [Fact]
    public void IsExpired_BeforeExpiry_ReturnsFalse()
    {
        var now = DateTimeOffset.UtcNow;
        var otp = IssueValid(now);

        Assert.False(otp.IsExpired(now.AddMinutes(4)));
    }

    [Fact]
    public void IsExpired_AtExpiry_ReturnsTrue()
    {
        var now = DateTimeOffset.UtcNow;
        var otp = IssueValid(now);

        Assert.True(otp.IsExpired(now.AddMinutes(5)));
    }

    [Fact]
    public void IsExpired_AfterExpiry_ReturnsTrue()
    {
        var now = DateTimeOffset.UtcNow;
        var otp = IssueValid(now);

        Assert.True(otp.IsExpired(now.AddMinutes(10)));
    }

    // --- CanBeUsed ---

    [Fact]
    public void CanBeUsed_WhenNotUsedAndNotExpired_ReturnsTrue()
    {
        var now = DateTimeOffset.UtcNow;
        var otp = IssueValid(now);

        Assert.True(otp.CanBeUsed(now.AddMinutes(1)));
    }

    [Fact]
    public void CanBeUsed_WhenUsed_ReturnsFalse()
    {
        var now = DateTimeOffset.UtcNow;
        var otp = IssueValid(now);
        otp.MarkUsed(now.AddMinutes(1));

        Assert.False(otp.CanBeUsed(now.AddMinutes(2)));
    }

    [Fact]
    public void CanBeUsed_WhenExpired_ReturnsFalse()
    {
        var now = DateTimeOffset.UtcNow;
        var otp = IssueValid(now);

        Assert.False(otp.CanBeUsed(now.AddMinutes(6)));
    }

    // --- MarkUsed ---

    [Fact]
    public void MarkUsed_WhenValid_SetsIsUsedAndUsedAt()
    {
        var now = DateTimeOffset.UtcNow;
        var otp = IssueValid(now);
        var useTime = now.AddMinutes(2);

        otp.MarkUsed(useTime);

        Assert.True(otp.IsUsed);
        Assert.Equal(useTime, otp.UsedAt);
    }

    [Fact]
    public void MarkUsed_WhenExpired_ThrowsBusinessRuleValidationException()
    {
        var now = DateTimeOffset.UtcNow;
        var otp = IssueValid(now);

        Assert.Throws<BusinessRuleValidationException>(() =>
            otp.MarkUsed(now.AddMinutes(10)));
    }

    [Fact]
    public void MarkUsed_WhenAlreadyUsed_ThrowsBusinessRuleValidationException()
    {
        var now = DateTimeOffset.UtcNow;
        var otp = IssueValid(now);
        otp.MarkUsed(now.AddMinutes(1));

        Assert.Throws<BusinessRuleValidationException>(() =>
            otp.MarkUsed(now.AddMinutes(2)));
    }
}

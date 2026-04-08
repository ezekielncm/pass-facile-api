using Domain.Aggregates.User;
using Domain.Common;
using Domain.DomainEvents.User;
using Domain.ValueObjects;

namespace DomainUnitTest.Aggregates;

public class UserTests
{
    private static readonly PhoneNumber ValidPhone = new("226", "70000001");

    private static User RegisterValid(UserProfile? profile = null) =>
        User.Register(ValidPhone, profile);

    // --- Register ---

    [Fact]
    public void Register_WithValidPhone_SetsPropertiesAndIsNotVerified()
    {
        var user = RegisterValid();

        Assert.Equal(ValidPhone, user.PhoneNumber);
        Assert.False(user.IsVerified);
        Assert.NotNull(user.Id);
        Assert.Null(user.Profile);
        Assert.Empty(user.Roles);
    }

    [Fact]
    public void Register_WithProfile_SetsDisplayNameFromProfile()
    {
        var profile = new UserProfile("John Doe", "Bio", "logo.png", "banner.png", "john-doe");

        var user = RegisterValid(profile);

        Assert.Equal("John Doe", user.DisplayName);
        Assert.Equal(profile, user.Profile);
    }

    [Fact]
    public void Register_RaisesUserRegistered()
    {
        var user = RegisterValid();

        Assert.Contains(user.Events, e => e is UserRegistered);
    }

    [Fact]
    public void Register_WithNullPhone_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => User.Register(null!));
    }

    // --- MarkOtpVerified ---

    [Fact]
    public void MarkOtpVerified_SetsIsVerifiedToTrue()
    {
        var user = RegisterValid();

        user.MarkOtpVerified();

        Assert.True(user.IsVerified);
    }

    [Fact]
    public void MarkOtpVerified_RaisesOtpVerified()
    {
        var user = RegisterValid();
        user.ClearEvents();

        user.MarkOtpVerified();

        Assert.Contains(user.Events, e => e is OtpVerified);
    }

    [Fact]
    public void MarkOtpVerified_WhenAlreadyVerified_IsIdempotent()
    {
        var user = RegisterValid();
        user.MarkOtpVerified();
        user.ClearEvents();

        user.MarkOtpVerified();

        Assert.True(user.IsVerified);
        Assert.Empty(user.Events);
    }

    // --- SetContextRole ---

    [Fact]
    public void SetContextRole_AddsNewRole()
    {
        var user = RegisterValid();

        user.SetContextRole("Organizer", "event-mgmt");

        Assert.Single(user.Roles);
        var role = user.Roles.First();
        Assert.Equal("Organizer", role.Name);
        Assert.Equal("event-mgmt", role.Context);
        Assert.True(role.IsActive);
    }

    [Fact]
    public void SetContextRole_RaisesUserRoleAssigned()
    {
        var user = RegisterValid();
        user.ClearEvents();

        user.SetContextRole("Admin", "platform");

        Assert.Contains(user.Events, e => e is UserRoleAssigned);
    }

    [Fact]
    public void SetContextRole_DeactivatesExistingRolesForSameContext()
    {
        var user = RegisterValid();
        user.SetContextRole("Agent", "access-control");
        user.SetContextRole("Admin", "access-control");

        var roles = user.Roles.ToList();
        Assert.Equal(2, roles.Count);
        Assert.Single(roles, r => r.IsActive && r.Name == "Admin");
        Assert.Single(roles, r => !r.IsActive && r.Name == "Agent");
    }

    [Fact]
    public void SetContextRole_DifferentContexts_BothActive()
    {
        var user = RegisterValid();
        user.SetContextRole("Agent", "access-control");
        user.SetContextRole("Organizer", "event-mgmt");

        var activeRoles = user.Roles.Where(r => r.IsActive).ToList();
        Assert.Equal(2, activeRoles.Count);
    }

    [Theory]
    [InlineData("", "context")]
    [InlineData(null, "context")]
    [InlineData("role", "")]
    [InlineData("role", null)]
    public void SetContextRole_WithEmptyOrNullArgs_ThrowsArgumentException(string? roleName, string? context)
    {
        var user = RegisterValid();

        Assert.Throws<ArgumentException>(() => user.SetContextRole(roleName!, context!));
    }

    // --- UpdateProfile ---

    [Fact]
    public void UpdateProfile_UpdatesProfileAndDisplayName()
    {
        var user = RegisterValid();
        var newProfile = new UserProfile("Jane Doe", "New bio", "new-logo.png", "new-banner.png", "jane-doe");

        user.UpdateProfile(newProfile);

        Assert.Equal(newProfile, user.Profile);
        Assert.Equal("Jane Doe", user.DisplayName);
    }

    [Fact]
    public void UpdateProfile_WithNull_ThrowsArgumentNullException()
    {
        var user = RegisterValid();

        Assert.Throws<ArgumentNullException>(() => user.UpdateProfile(null!));
    }
}

using Domain.Aggregates.User;
using Domain.ValueObjects.Identities;

namespace DomainUnitTest
{
    public class RefreshTokenTests
    {
        [Fact]
        public void Create_ShouldSetProperties()
        {
            var userId = UserId.NewId();
            var expiresAt = DateTimeOffset.UtcNow.AddDays(7);

            var token = RefreshToken.Create("abc123", userId, expiresAt);

            Assert.Equal("abc123", token.Token);
            Assert.Equal(userId, token.UserId);
            Assert.Equal(expiresAt, token.ExpiresAt);
            Assert.False(token.IsRevoked);
        }

        [Fact]
        public void IsExpired_WhenBeforeExpiry_ReturnsFalse()
        {
            var token = RefreshToken.Create("abc123", UserId.NewId(), DateTimeOffset.UtcNow.AddDays(7));

            Assert.False(token.IsExpired(DateTimeOffset.UtcNow));
        }

        [Fact]
        public void IsExpired_WhenAfterExpiry_ReturnsTrue()
        {
            var token = RefreshToken.Create("abc123", UserId.NewId(), DateTimeOffset.UtcNow.AddDays(-1));

            Assert.True(token.IsExpired(DateTimeOffset.UtcNow));
        }

        [Fact]
        public void Revoke_ShouldMarkAsRevoked()
        {
            var token = RefreshToken.Create("abc123", UserId.NewId(), DateTimeOffset.UtcNow.AddDays(7));

            token.Revoke();

            Assert.True(token.IsRevoked);
        }

        [Fact]
        public void Rotate_ShouldRevokeCurrentAndCreateNew()
        {
            var userId = UserId.NewId();
            var token = RefreshToken.Create("old-token", userId, DateTimeOffset.UtcNow.AddDays(7));
            var newExpiresAt = DateTimeOffset.UtcNow.AddDays(14);

            var newToken = token.Rotate("new-token", newExpiresAt);

            Assert.True(token.IsRevoked);
            Assert.Equal("new-token", newToken.Token);
            Assert.Equal(userId, newToken.UserId);
            Assert.Equal(newExpiresAt, newToken.ExpiresAt);
            Assert.False(newToken.IsRevoked);
        }

        [Fact]
        public void Create_WithEmptyToken_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() =>
                RefreshToken.Create("", UserId.NewId(), DateTimeOffset.UtcNow.AddDays(7)));
        }
    }
}

using Domain.Aggregates.AccessControl;

namespace DomainUnitTest
{
    public class OfflineBundleTests
    {
        [Fact]
        public void Create_ShouldSetProperties()
        {
            var eventId = Guid.NewGuid();
            var tickets = new[] { "QR1", "QR2", "QR3" };
            var now = DateTimeOffset.UtcNow;
            var validity = TimeSpan.FromHours(12);

            var bundle = OfflineBundle.Create(eventId, tickets, "signature123", now, validity);

            Assert.Equal(eventId, bundle.EventId);
            Assert.Equal(3, bundle.Tickets.Count);
            Assert.Equal("signature123", bundle.Signature);
            Assert.Equal(now, bundle.GeneratedAt);
            Assert.Equal(now.Add(validity), bundle.ExpiresAt);
        }

        [Fact]
        public void IsExpired_WhenBeforeExpiry_ReturnsFalse()
        {
            var now = DateTimeOffset.UtcNow;
            var bundle = OfflineBundle.Create(Guid.NewGuid(), ["QR1"], "sig", now, TimeSpan.FromHours(12));

            Assert.False(bundle.IsExpired(now));
        }

        [Fact]
        public void IsExpired_WhenAfterExpiry_ReturnsTrue()
        {
            var now = DateTimeOffset.UtcNow;
            var bundle = OfflineBundle.Create(Guid.NewGuid(), ["QR1"], "sig", now, TimeSpan.FromHours(1));

            Assert.True(bundle.IsExpired(now.AddHours(2)));
        }

        [Fact]
        public void Validate_WhenNotExpiredAndHasSignature_ReturnsTrue()
        {
            var now = DateTimeOffset.UtcNow;
            var bundle = OfflineBundle.Create(Guid.NewGuid(), ["QR1"], "sig", now, TimeSpan.FromHours(12));

            Assert.True(bundle.Validate(now));
        }

        [Fact]
        public void Validate_WhenExpired_ReturnsFalse()
        {
            var now = DateTimeOffset.UtcNow;
            var bundle = OfflineBundle.Create(Guid.NewGuid(), ["QR1"], "sig", now, TimeSpan.FromHours(1));

            Assert.False(bundle.Validate(now.AddHours(2)));
        }

        [Fact]
        public void Create_WithEmptySignature_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() =>
                OfflineBundle.Create(Guid.NewGuid(), ["QR1"], "", DateTimeOffset.UtcNow, TimeSpan.FromHours(1)));
        }
    }
}

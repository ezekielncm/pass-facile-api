using Domain.Aggregates.AccessControl;

namespace DomainUnitTest
{
    public class AgentAssignmentTests
    {
        [Fact]
        public void Create_ShouldSetProperties()
        {
            var eventId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var assignment = AgentAssignment.Create(eventId, agentId, now);

            Assert.Equal(eventId, assignment.EventId);
            Assert.Equal(agentId, assignment.AgentId);
            Assert.Equal(now, assignment.AssignedAt);
            Assert.True(assignment.IsActive);
            Assert.Null(assignment.RevokedAt);
        }

        [Fact]
        public void Revoke_ShouldDeactivateAndSetRevokedAt()
        {
            var assignment = AgentAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var revokedAt = DateTimeOffset.UtcNow.AddHours(1);

            assignment.Revoke(revokedAt);

            Assert.False(assignment.IsActive);
            Assert.Equal(revokedAt, assignment.RevokedAt);
        }

        [Fact]
        public void Revoke_WhenAlreadyRevoked_ShouldBeIdempotent()
        {
            var assignment = AgentAssignment.Create(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
            var firstRevoke = DateTimeOffset.UtcNow.AddHours(1);
            var secondRevoke = DateTimeOffset.UtcNow.AddHours(2);

            assignment.Revoke(firstRevoke);
            assignment.Revoke(secondRevoke);

            Assert.False(assignment.IsActive);
            Assert.Equal(firstRevoke, assignment.RevokedAt);
        }
    }
}

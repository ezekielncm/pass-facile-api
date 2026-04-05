using Domain.Aggregates.AccessControl;

namespace Application.Scan.DTOs;

public sealed record AgentAssignmentDto(
    Guid Id,
    Guid EventId,
    Guid AgentId,
    DateTimeOffset AssignedAt,
    bool IsActive)
{
    public static AgentAssignmentDto FromDomain(AgentAssignment assignment)
    {
        return new AgentAssignmentDto(
            assignment.Id,
            assignment.EventId,
            assignment.AgentId,
            assignment.AssignedAt,
            assignment.IsActive);
    }
}

using Application.Common.Models;
using MediatR;

namespace Application.Scan.Commands.RevokeAgent;

public sealed record RevokeAgentCommand(
    Guid EventId,
    Guid AgentId) : IRequest<Result<bool>>;

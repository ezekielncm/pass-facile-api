using Application.Common.Models;
using Application.Scan.DTOs;
using MediatR;

namespace Application.Scan.Commands.AssignAgent;

public sealed record AssignAgentCommand(
    Guid EventId,
    string AgentPhone) : IRequest<Result<AgentAssignmentDto>>;

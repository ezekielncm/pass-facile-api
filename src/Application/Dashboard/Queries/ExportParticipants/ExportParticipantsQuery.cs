using Application.Common.Models;
using Application.Dashboard.DTOs;
using MediatR;

namespace Application.Dashboard.Queries.ExportParticipants;

public sealed record ExportParticipantsQuery(
    Guid EventId,
    string Format = "csv",
    Guid? CategoryId = null) : IRequest<Result<ParticipantExportDto>>;

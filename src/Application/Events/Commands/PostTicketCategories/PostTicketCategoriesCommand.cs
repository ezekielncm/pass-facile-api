using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;

namespace Application.Events.Commands.PostTicketCategories;

public sealed record PostTicketCategoryCommand(
    Guid EventId,
    string Name,
    decimal Price,
    int Quota,
    string? Description,
    string FeePolicy) : IRequest<Result<CategoryDto>>;

using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;

namespace Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    string? Name,
    decimal? Price,
    int? Quota,
    bool? IsActive) : IRequest<Result<CategoryDto>>;

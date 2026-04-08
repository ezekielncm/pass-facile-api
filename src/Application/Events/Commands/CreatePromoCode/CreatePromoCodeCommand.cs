using Application.Common.Models;
using Application.Events.DTOs;
using MediatR;

namespace Application.Events.Commands.CreatePromoCode;

public sealed record CreatePromoCodeCommand(
    Guid EventId,
    string Code,
    string DiscountType,
    decimal Value,
    int MaxUses,
    DateTimeOffset ExpiresAt) : IRequest<Result<PromoCodeDto>>;

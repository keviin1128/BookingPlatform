using BookingPlatform.Application.Loyalty.DTOs;
using BookingPlatform.Domain.Enums;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Queries.GetLoyalty;

public record GetLoyaltyQuery(
    Guid UserId,
    Guid RequesterUserId,
    Role RequesterRole) : IRequest<LoyaltyDto>;
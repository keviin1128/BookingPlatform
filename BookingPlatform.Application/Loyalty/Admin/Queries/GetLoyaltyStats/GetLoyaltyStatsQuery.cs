using BookingPlatform.Application.Loyalty.Admin.DTOs;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyStats;

public record GetLoyaltyStatsQuery() : IRequest<AdminLoyaltyStatsDto>;

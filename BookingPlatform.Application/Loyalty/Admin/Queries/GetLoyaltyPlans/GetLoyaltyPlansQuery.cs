using BookingPlatform.Application.Loyalty.Admin.DTOs;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyPlans;

public record GetLoyaltyPlansQuery() : IRequest<IReadOnlyList<AdminLoyaltyPlanDto>>;

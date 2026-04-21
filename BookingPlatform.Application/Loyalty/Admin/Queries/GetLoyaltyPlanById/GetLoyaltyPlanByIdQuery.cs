using BookingPlatform.Application.Loyalty.Admin.DTOs;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyPlanById;

public record GetLoyaltyPlanByIdQuery(Guid PlanId) : IRequest<AdminLoyaltyPlanDto>;

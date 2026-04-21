using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Loyalty.Admin.DTOs;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyPlanById;

public class GetLoyaltyPlanByIdQueryHandler : IRequestHandler<GetLoyaltyPlanByIdQuery, AdminLoyaltyPlanDto>
{
    private readonly ILoyaltyRepository _loyaltyRepository;

    public GetLoyaltyPlanByIdQueryHandler(ILoyaltyRepository loyaltyRepository)
    {
        _loyaltyRepository = loyaltyRepository;
    }

    public async Task<AdminLoyaltyPlanDto> Handle(GetLoyaltyPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await _loyaltyRepository.GetPlanByIdAsync(request.PlanId, cancellationToken)
            ?? throw new KeyNotFoundException("Plan de lealtad no encontrado.");

        return AdminLoyaltyPlanDto.FromDomain(plan);
    }
}

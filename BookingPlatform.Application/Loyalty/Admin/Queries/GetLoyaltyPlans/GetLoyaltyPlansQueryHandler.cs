using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Loyalty.Admin.DTOs;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyPlans;

public class GetLoyaltyPlansQueryHandler : IRequestHandler<GetLoyaltyPlansQuery, IReadOnlyList<AdminLoyaltyPlanDto>>
{
    private readonly ILoyaltyRepository _loyaltyRepository;

    public GetLoyaltyPlansQueryHandler(ILoyaltyRepository loyaltyRepository)
    {
        _loyaltyRepository = loyaltyRepository;
    }

    public async Task<IReadOnlyList<AdminLoyaltyPlanDto>> Handle(GetLoyaltyPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _loyaltyRepository.GetPlansAsync(cancellationToken);

        return plans
            .OrderByDescending(x => x.Activo)
            .ThenBy(x => x.Nombre)
            .Select(AdminLoyaltyPlanDto.FromDomain)
            .ToList();
    }
}

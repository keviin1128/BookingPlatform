using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Loyalty.Admin.DTOs;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyStats;

public class GetLoyaltyStatsQueryHandler : IRequestHandler<GetLoyaltyStatsQuery, AdminLoyaltyStatsDto>
{
    private readonly ILoyaltyRepository _loyaltyRepository;

    public GetLoyaltyStatsQueryHandler(ILoyaltyRepository loyaltyRepository)
    {
        _loyaltyRepository = loyaltyRepository;
    }

    public async Task<AdminLoyaltyStatsDto> Handle(GetLoyaltyStatsQuery request, CancellationToken cancellationToken)
    {
        var plans = await _loyaltyRepository.GetPlansAsync(cancellationToken);
        var totalAccounts = await _loyaltyRepository.GetTotalAccountsAsync(cancellationToken);
        var totalRedemptions = await _loyaltyRepository.GetTotalRedemptionsAsync(cancellationToken);
        var averagePoints = await _loyaltyRepository.GetAveragePointsPerAccountAsync(cancellationToken);

        return new AdminLoyaltyStatsDto(
            plans.Count,
            plans.Count(x => x.Activo),
            totalAccounts,
            totalRedemptions,
            Math.Round(averagePoints, 2));
    }
}

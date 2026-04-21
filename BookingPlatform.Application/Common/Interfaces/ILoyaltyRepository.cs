using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Common.Interfaces;

public interface ILoyaltyRepository
{
    Task<LoyaltyPlan> GetOrCreateActivePlanAsync(CancellationToken cancellationToken = default);

    Task<LoyaltyAccount> GetOrCreateAccountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<LoyaltyReward?> GetRewardByIdAsync(Guid rewardId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LoyaltyPlan>> GetPlansAsync(CancellationToken cancellationToken = default);

    Task<LoyaltyPlan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default);

    Task AddPlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default);

    Task UpdatePlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default);

    Task DeletePlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LoyaltyRedemption>> GetRedemptionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<int> GetTotalAccountsAsync(CancellationToken cancellationToken = default);

    Task<int> GetTotalRedemptionsAsync(CancellationToken cancellationToken = default);

    Task<decimal> GetAveragePointsPerAccountAsync(CancellationToken cancellationToken = default);

    Task UpdateAccountAsync(LoyaltyAccount account, CancellationToken cancellationToken = default);

    Task UpdateRewardAsync(LoyaltyReward reward, CancellationToken cancellationToken = default);

    Task AddRedemptionAsync(LoyaltyRedemption redemption, CancellationToken cancellationToken = default);
}
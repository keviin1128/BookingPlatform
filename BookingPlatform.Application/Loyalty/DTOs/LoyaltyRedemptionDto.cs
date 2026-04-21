using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Loyalty.DTOs;

public record LoyaltyRedemptionDto(
    Guid Id,
    Guid RewardId,
    string RewardName,
    int PointsSpent,
    string RedeemedAt)
{
    public static LoyaltyRedemptionDto FromDomain(LoyaltyRedemption redemption)
    {
        return new LoyaltyRedemptionDto(
            redemption.Id,
            redemption.RewardId,
            redemption.RewardName,
            redemption.PointsSpent,
            redemption.RedeemedAt.ToString("yyyy-MM-dd HH:mm"));
    }
}
namespace BookingPlatform.Domain.Entities;

public class LoyaltyRedemption
{
    public Guid Id { get; set; }

    public Guid LoyaltyAccountId { get; set; }

    public Guid RewardId { get; set; }

    public string RewardName { get; set; } = string.Empty;

    public int PointsSpent { get; set; }

    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

    public LoyaltyAccount LoyaltyAccount { get; set; } = null!;

    public LoyaltyReward Reward { get; set; } = null!;
}
namespace BookingPlatform.Domain.Entities;

public class LoyaltyAccount
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid PlanId { get; set; }

    public int Points { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public LoyaltyPlan Plan { get; set; } = null!;

    public ICollection<LoyaltyRedemption> Redemptions { get; set; } = new List<LoyaltyRedemption>();
}
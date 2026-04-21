namespace BookingPlatform.Domain.Entities;

public class LoyaltyLevel
{
    public Guid Id { get; set; }

    public Guid PlanId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int MinPoints { get; set; }

    public int? MaxPoints { get; set; }

    public int Orden { get; set; }

    public LoyaltyPlan Plan { get; set; } = null!;
}
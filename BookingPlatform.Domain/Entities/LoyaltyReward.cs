namespace BookingPlatform.Domain.Entities;

public class LoyaltyReward
{
    public Guid Id { get; set; }

    public Guid PlanId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int PuntosRequeridos { get; set; }

    public int CantidadDisponible { get; set; }

    public bool Activo { get; set; } = true;

    public LoyaltyPlan Plan { get; set; } = null!;
}
namespace BookingPlatform.Domain.Entities;

public class LoyaltyPlan
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int PuntosPorCita { get; set; } = 1;

    public decimal PuntosPorDolar { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<LoyaltyLevel> Levels { get; set; } = new List<LoyaltyLevel>();

    public ICollection<LoyaltyReward> Rewards { get; set; } = new List<LoyaltyReward>();

    public static LoyaltyPlan CreateDefault()
    {
        var plan = new LoyaltyPlan
        {
            Id = Guid.NewGuid(),
            Nombre = "Plan lealtad",
            PuntosPorCita = 1,
            PuntosPorDolar = 0m,
            Activo = true
        };

        plan.Levels = new List<LoyaltyLevel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                Nombre = "Nivel 1",
                MinPoints = 0,
                MaxPoints = 4,
                Orden = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                Nombre = "Nivel 2",
                MinPoints = 5,
                MaxPoints = 9,
                Orden = 2
            },
            new()
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                Nombre = "Nivel 3",
                MinPoints = 10,
                MaxPoints = null,
                Orden = 3
            }
        };

        plan.Rewards = new List<LoyaltyReward>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                Nombre = "Descuento de bienvenida",
                PuntosRequeridos = 5,
                CantidadDisponible = 50,
                Activo = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                Nombre = "Servicio gratis",
                PuntosRequeridos = 10,
                CantidadDisponible = 10,
                Activo = true
            }
        };

        return plan;
    }
}
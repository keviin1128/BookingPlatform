using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Loyalty.Admin.DTOs;

public record AdminLoyaltyPlanDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    int PuntosXCita,
    decimal PuntosXDolar,
    IReadOnlyList<AdminLoyaltyLevelDto> Niveles,
    bool Activo,
    DateTime? CreatedAt)
{
    public static AdminLoyaltyPlanDto FromDomain(LoyaltyPlan plan)
    {
        return new AdminLoyaltyPlanDto(
            plan.Id,
            plan.Nombre,
            string.Empty,
            plan.PuntosPorCita,
            plan.PuntosPorDolar,
            plan.Levels
                .OrderBy(x => x.Orden)
                .Select(AdminLoyaltyLevelDto.FromDomain)
                .ToList(),
            plan.Activo,
            null);
    }
}

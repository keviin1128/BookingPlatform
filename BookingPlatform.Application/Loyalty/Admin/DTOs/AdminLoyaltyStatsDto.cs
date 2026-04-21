namespace BookingPlatform.Application.Loyalty.Admin.DTOs;

public record AdminLoyaltyStatsDto(
    int PlanesTotales,
    int PlanesActivos,
    int CuentasLealtadTotales,
    int CanjesTotales,
    decimal PuntosPromedioPorCuenta);

namespace BookingPlatform.Application.Loyalty.Admin.DTOs;

public record LoyaltyPlanLevelInputDto(
    string Nombre,
    int PuntosMinimos,
    int? PuntosMaximos);

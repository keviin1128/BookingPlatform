using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Loyalty.Admin.DTOs;

public record AdminLoyaltyLevelDto(
    string Nombre,
    int PuntosMinimos,
    int? PuntosMaximos)
{
    public static AdminLoyaltyLevelDto FromDomain(LoyaltyLevel level)
    {
        return new AdminLoyaltyLevelDto(level.Nombre, level.MinPoints, level.MaxPoints);
    }
}

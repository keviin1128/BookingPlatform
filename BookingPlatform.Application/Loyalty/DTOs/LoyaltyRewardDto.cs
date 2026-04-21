using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Loyalty.DTOs;

public record LoyaltyRewardDto(
    Guid Id,
    string Nombre,
    int PuntosRequeridos,
    int CantidadDisponible,
    bool Disponible)
{
    public static LoyaltyRewardDto FromDomain(LoyaltyReward reward, int currentPoints)
    {
        return new LoyaltyRewardDto(
            reward.Id,
            reward.Nombre,
            reward.PuntosRequeridos,
            reward.CantidadDisponible,
            reward.Activo && reward.CantidadDisponible > 0 && currentPoints >= reward.PuntosRequeridos);
    }
}
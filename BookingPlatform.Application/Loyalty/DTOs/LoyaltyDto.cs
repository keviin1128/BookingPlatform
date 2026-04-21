namespace BookingPlatform.Application.Loyalty.DTOs;

public record LoyaltyDto(
    Guid UsuarioId,
    int Puntos,
    string Nivel,
    string? SiguienteNivel,
    int PuntosParaSiguienteNivel,
    int Progreso,
    IReadOnlyList<LoyaltyRewardDto> RecompensasDisponibles,
    IReadOnlyList<LoyaltyRedemptionDto> HistorialCanjes);
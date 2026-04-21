using BookingPlatform.Application.Loyalty.Admin.DTOs;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Admin.Commands.CreateLoyaltyPlan;

public record CreateLoyaltyPlanCommand(
    string Nombre,
    string? Descripcion,
    int PuntosXCita,
    decimal PuntosXDolar,
    IReadOnlyList<LoyaltyPlanLevelInputDto> Niveles,
    bool Activo = true) : IRequest<AdminLoyaltyPlanDto>;

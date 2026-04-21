using BookingPlatform.Application.Loyalty.Admin.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace BookingPlatform.Application.Loyalty.Admin.Commands.UpdateLoyaltyPlan;

public record UpdateLoyaltyPlanCommand : IRequest<AdminLoyaltyPlanDto>
{
    [JsonIgnore]
    public Guid PlanId { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string? Descripcion { get; init; }

    public int PuntosXCita { get; init; }

    public decimal PuntosXDolar { get; init; }

    public IReadOnlyList<LoyaltyPlanLevelInputDto> Niveles { get; init; } = Array.Empty<LoyaltyPlanLevelInputDto>();

    public bool Activo { get; init; }
}

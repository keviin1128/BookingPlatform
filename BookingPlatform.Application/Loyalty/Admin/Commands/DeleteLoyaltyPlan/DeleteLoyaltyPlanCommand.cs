using MediatR;
using System.Text.Json.Serialization;

namespace BookingPlatform.Application.Loyalty.Admin.Commands.DeleteLoyaltyPlan;

public record DeleteLoyaltyPlanCommand : IRequest<Unit>
{
    [JsonIgnore]
    public Guid PlanId { get; init; }
}

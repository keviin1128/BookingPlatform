using MediatR;
using System.Text.Json.Serialization;

namespace BookingPlatform.Application.Services.Commands.DeleteService;

public record DeleteServiceCommand : IRequest<Unit>
{
    [JsonIgnore]
    public Guid Id { get; init; }
}

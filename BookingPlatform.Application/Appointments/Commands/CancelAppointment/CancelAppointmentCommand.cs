using MediatR;
using BookingPlatform.Domain.Enums;
using System.Text.Json.Serialization;

namespace BookingPlatform.Application.Appointments.Commands.CancelAppointment;

public record CancelAppointmentCommand : IRequest<Unit>
{
    [JsonIgnore]
    public Guid Id { get; init; }

    [JsonIgnore]
    public Guid RequesterUserId { get; init; }

    [JsonIgnore]
    public Role RequesterRole { get; init; }
}

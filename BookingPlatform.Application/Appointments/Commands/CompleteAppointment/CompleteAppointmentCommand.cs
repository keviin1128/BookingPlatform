using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Domain.Enums;
using MediatR;
using System.Text.Json.Serialization;

namespace BookingPlatform.Application.Appointments.Commands.CompleteAppointment;

public record CompleteAppointmentCommand : IRequest<AppointmentDto>
{
    [JsonIgnore]
    public Guid Id { get; init; }

    [JsonIgnore]
    public Guid RequesterUserId { get; init; }

    [JsonIgnore]
    public Role RequesterRole { get; init; }
}

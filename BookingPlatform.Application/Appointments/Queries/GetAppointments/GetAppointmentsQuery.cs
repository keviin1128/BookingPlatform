using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Domain.Enums;
using MediatR;

namespace BookingPlatform.Application.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery(Role RequesterRole, Guid RequesterUserId) : IRequest<IReadOnlyList<AppointmentDto>>;

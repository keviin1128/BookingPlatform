using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Domain.Enums;
using MediatR;

namespace BookingPlatform.Application.Appointments.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery(Guid AppointmentId, Role RequesterRole, Guid RequesterUserId) : IRequest<AppointmentDto>;

using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Enums;
using MediatR;

namespace BookingPlatform.Application.Appointments.Queries.GetAppointmentById;

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IWorkerRepository _workerRepository;

    public GetAppointmentByIdQueryHandler(
        IAppointmentRepository appointmentRepository,
        IWorkerRepository workerRepository)
    {
        _appointmentRepository = appointmentRepository;
        _workerRepository = workerRepository;
    }

    public async Task<AppointmentDto> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
            ?? throw new KeyNotFoundException("Cita no encontrada.");

        if (request.RequesterRole == Role.Admin)
        {
            return AppointmentDto.FromDomain(appointment);
        }

        if (request.RequesterRole == Role.Customer)
        {
            if (appointment.ClienteId != request.RequesterUserId)
            {
                throw new UnauthorizedAccessException("No tienes permisos para ver esta cita.");
            }

            return AppointmentDto.FromDomain(appointment);
        }

        if (request.RequesterRole == Role.Worker)
        {
            var worker = await _workerRepository.GetByUserIdAsync(request.RequesterUserId, cancellationToken)
                ?? throw new UnauthorizedAccessException("No tienes permisos para ver esta cita.");

            if (appointment.TrabajadorId != worker.Id)
            {
                throw new UnauthorizedAccessException("No tienes permisos para ver esta cita.");
            }

            return AppointmentDto.FromDomain(appointment);
        }

        throw new UnauthorizedAccessException("No tienes permisos para ver esta cita.");
    }
}

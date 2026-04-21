using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using MediatR;

namespace BookingPlatform.Application.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IWorkerRepository _workerRepository;

    public GetAppointmentsQueryHandler(
        IAppointmentRepository appointmentRepository,
        IWorkerRepository workerRepository)
    {
        _appointmentRepository = appointmentRepository;
        _workerRepository = workerRepository;
    }

    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Appointment> appointments = request.RequesterRole switch
        {
            Role.Admin => await _appointmentRepository.GetAllAsync(cancellationToken),
            Role.Customer => await _appointmentRepository.GetByCustomerIdAsync(request.RequesterUserId, cancellationToken),
            Role.Worker => await GetWorkerAppointmentsAsync(request.RequesterUserId, cancellationToken),
            _ => Array.Empty<Appointment>()
        };

        return appointments
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Hora)
            .Select(AppointmentDto.FromDomain)
            .ToList();
    }

    private async Task<IReadOnlyList<Appointment>> GetWorkerAppointmentsAsync(Guid requesterUserId, CancellationToken cancellationToken)
    {
        var worker = await _workerRepository.GetByUserIdAsync(requesterUserId, cancellationToken);
        if (worker is null)
        {
            return Array.Empty<Appointment>();
        }

        return await _appointmentRepository.GetByWorkerIdAsync(worker.Id, cancellationToken);
    }
}

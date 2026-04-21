using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Enums;
using MediatR;

namespace BookingPlatform.Application.Appointments.Queries.GetAvailableSlots;

public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<string>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IWorkerRepository _workerRepository;

    public GetAvailableSlotsQueryHandler(
        IAppointmentRepository appointmentRepository,
        IServiceRepository serviceRepository,
        IWorkerRepository workerRepository)
    {
        _appointmentRepository = appointmentRepository;
        _serviceRepository = serviceRepository;
        _workerRepository = workerRepository;
    }

    public async Task<IReadOnlyList<string>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        var worker = await _workerRepository.GetByIdAsync(request.WorkerId, cancellationToken)
            ?? throw new KeyNotFoundException("Trabajador no encontrado.");

        if (!worker.WorkerServices.Any(ws => ws.ServiceId == request.ServiceId))
        {
            throw new InvalidOperationException("El trabajador no ofrece el servicio seleccionado.");
        }

        var duration = request.Duration ?? service.Duracion;
        if (duration <= 0)
        {
            throw new InvalidOperationException("La duracion debe ser mayor a 0.");
        }

        var dayOfWeek = (int)request.Date.DayOfWeek;
        var ranges = worker.Agenda
            .Where(x => x.DayOfWeek == dayOfWeek)
            .Select(x => new
            {
                Start = TimeOnly.ParseExact(x.StartTime, "HH:mm"),
                End = TimeOnly.ParseExact(x.EndTime, "HH:mm")
            })
            .OrderBy(x => x.Start)
            .ToList();

        if (ranges.Count == 0)
        {
            return Array.Empty<string>();
        }

        var appointments = await _appointmentRepository
            .GetByWorkerAndDateAsync(worker.Id, request.Date, cancellationToken);

        var busySlots = appointments
            .Where(x => x.Estado != AppointmentStatus.Cancelled)
            .Select(x => new
            {
                Start = x.Hora,
                End = x.Hora.AddMinutes(x.Duracion)
            })
            .ToList();

        var result = new List<string>();

        foreach (var range in ranges)
        {
            var cursor = range.Start;
            var lastStart = range.End.AddMinutes(-duration);

            while (cursor <= lastStart)
            {
                var candidateEnd = cursor.AddMinutes(duration);

                var overlaps = busySlots.Any(busy => cursor < busy.End && busy.Start < candidateEnd);
                if (!overlaps)
                {
                    result.Add(cursor.ToString("HH:mm"));
                }

                cursor = cursor.AddMinutes(30);
            }
        }

        return result
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }
}

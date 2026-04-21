using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Workers.DTOs;

public record WorkerDto(
    Guid Id,
    Guid UserId,
    string Nombre,
    string Especialidad,
    IReadOnlyList<Guid> Servicios,
    string HorarioResumen,
    Dictionary<int, IReadOnlyList<ScheduleRangeDto>>? Agenda)
{
    public static WorkerDto FromDomain(Worker worker, bool includeAgenda)
    {
        var services = worker.WorkerServices
            .Select(x => x.ServiceId)
            .Distinct()
            .ToList();

        Dictionary<int, IReadOnlyList<ScheduleRangeDto>>? agenda = null;
        if (includeAgenda)
        {
            agenda = worker.Agenda
                .GroupBy(x => x.DayOfWeek)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<ScheduleRangeDto>)group
                        .OrderBy(x => x.StartTime)
                        .Select(x => new ScheduleRangeDto(x.StartTime, x.EndTime))
                        .ToList());
        }

        return new WorkerDto(
            worker.Id,
            worker.UserId,
            worker.User.Nombre ?? string.Empty,
            worker.Especialidad,
            services,
            worker.HorarioResumen,
            agenda);
    }
}

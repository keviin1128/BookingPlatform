using BookingPlatform.Application.Workers.DTOs;
using MediatR;

namespace BookingPlatform.Application.Workers.Commands.CreateWorker;

public record CreateWorkerCommand(
    string Telefono,
    string? Nombre,
    string? Email,
    string? Especialidad,
    IReadOnlyList<Guid>? Servicios,
    Dictionary<int, IReadOnlyList<ScheduleRangeDto>>? Agenda) : IRequest<WorkerDto>;

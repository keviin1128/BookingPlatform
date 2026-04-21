using BookingPlatform.Application.Workers.DTOs;
using MediatR;

namespace BookingPlatform.Application.Workers.Queries.GetWorkers;

public record GetWorkersQuery(string? Search, Guid? ServiceId) : IRequest<IReadOnlyList<WorkerDto>>;

using BookingPlatform.Application.Workers.DTOs;
using MediatR;

namespace BookingPlatform.Application.Workers.Queries.GetWorkerById;

public record GetWorkerByIdQuery(Guid Id) : IRequest<WorkerDto>;

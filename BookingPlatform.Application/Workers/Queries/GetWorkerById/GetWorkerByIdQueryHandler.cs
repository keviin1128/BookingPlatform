using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Workers.DTOs;
using MediatR;

namespace BookingPlatform.Application.Workers.Queries.GetWorkerById;

public class GetWorkerByIdQueryHandler : IRequestHandler<GetWorkerByIdQuery, WorkerDto>
{
    private readonly IWorkerRepository _workerRepository;

    public GetWorkerByIdQueryHandler(IWorkerRepository workerRepository)
    {
        _workerRepository = workerRepository;
    }

    public async Task<WorkerDto> Handle(GetWorkerByIdQuery request, CancellationToken cancellationToken)
    {
        var worker = await _workerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (worker is null)
        {
            throw new KeyNotFoundException("Trabajador no encontrado.");
        }

        return WorkerDto.FromDomain(worker, includeAgenda: true);
    }
}

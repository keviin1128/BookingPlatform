using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Workers.DTOs;
using MediatR;

namespace BookingPlatform.Application.Workers.Queries.GetWorkers;

public class GetWorkersQueryHandler : IRequestHandler<GetWorkersQuery, IReadOnlyList<WorkerDto>>
{
    private readonly IWorkerRepository _workerRepository;

    public GetWorkersQueryHandler(IWorkerRepository workerRepository)
    {
        _workerRepository = workerRepository;
    }

    public async Task<IReadOnlyList<WorkerDto>> Handle(GetWorkersQuery request, CancellationToken cancellationToken)
    {
        var workers = await _workerRepository.GetAllAsync(request.Search, request.ServiceId, cancellationToken);

        return workers
            .OrderBy(x => x.User.Nombre)
            .Select(worker => WorkerDto.FromDomain(worker, includeAgenda: false))
            .ToList();
    }
}

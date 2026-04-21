using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Services.DTOs;
using MediatR;

namespace BookingPlatform.Application.Services.Queries.GetServices;

public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, IReadOnlyList<ServiceDto>>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServicesQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<IReadOnlyList<ServiceDto>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _serviceRepository.GetAllAsync(cancellationToken);

        return services
            .OrderBy(x => x.Nombre)
            .Select(ServiceDto.FromDomain)
            .ToList();
    }
}

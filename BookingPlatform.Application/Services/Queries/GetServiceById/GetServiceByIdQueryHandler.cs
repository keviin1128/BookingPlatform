using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Services.DTOs;
using MediatR;

namespace BookingPlatform.Application.Services.Queries.GetServiceById;

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceDto>
{
    private readonly IServiceRepository _serviceRepository;

    public GetServiceByIdQueryHandler(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<ServiceDto> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (service is null)
        {
            throw new KeyNotFoundException("Servicio no encontrado.");
        }

        return ServiceDto.FromDomain(service);
    }
}

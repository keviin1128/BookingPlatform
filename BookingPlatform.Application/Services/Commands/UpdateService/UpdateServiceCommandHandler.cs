using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Services.DTOs;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Services.Commands.UpdateService;

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ServiceDto>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IValidator<UpdateServiceCommand> _validator;

    public UpdateServiceCommandHandler(
        IServiceRepository serviceRepository,
        IValidator<UpdateServiceCommand> validator)
    {
        _serviceRepository = serviceRepository;
        _validator = validator;
    }

    public async Task<ServiceDto> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var service = await _serviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (service is null)
        {
            throw new KeyNotFoundException("Servicio no encontrado.");
        }

        service.Nombre = request.Nombre.Trim();
        service.Descripcion = request.Descripcion.Trim();
        service.Duracion = request.Duracion;
        service.Precio = request.Precio;
        service.Activo = request.Activo;

        await _serviceRepository.UpdateAsync(service, cancellationToken);

        return ServiceDto.FromDomain(service);
    }
}

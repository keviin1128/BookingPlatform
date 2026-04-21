using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Services.DTOs;
using BookingPlatform.Domain.Entities;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Services.Commands.CreateService;

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ServiceDto>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IValidator<CreateServiceCommand> _validator;

    public CreateServiceCommandHandler(
        IServiceRepository serviceRepository,
        IValidator<CreateServiceCommand> validator)
    {
        _serviceRepository = serviceRepository;
        _validator = validator;
    }

    public async Task<ServiceDto> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var service = new Service
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion.Trim(),
            Duracion = request.Duracion,
            Precio = request.Precio,
            Activo = request.Activo
        };

        await _serviceRepository.AddAsync(service, cancellationToken);

        return ServiceDto.FromDomain(service);
    }
}

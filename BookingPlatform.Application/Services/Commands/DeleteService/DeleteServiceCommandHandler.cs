using BookingPlatform.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Services.Commands.DeleteService;

public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, Unit>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IValidator<DeleteServiceCommand> _validator;

    public DeleteServiceCommandHandler(
        IServiceRepository serviceRepository,
        IValidator<DeleteServiceCommand> validator)
    {
        _serviceRepository = serviceRepository;
        _validator = validator;
    }

    public async Task<Unit> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var service = await _serviceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (service is null)
        {
            throw new KeyNotFoundException("Servicio no encontrado.");
        }

        await _serviceRepository.DeleteAsync(service, cancellationToken);

        return Unit.Value;
    }
}

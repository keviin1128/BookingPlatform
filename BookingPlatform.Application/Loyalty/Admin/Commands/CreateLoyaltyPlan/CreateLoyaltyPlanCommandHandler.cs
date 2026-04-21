using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Loyalty.Admin.DTOs;
using BookingPlatform.Domain.Entities;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Admin.Commands.CreateLoyaltyPlan;

public class CreateLoyaltyPlanCommandHandler : IRequestHandler<CreateLoyaltyPlanCommand, AdminLoyaltyPlanDto>
{
    private readonly ILoyaltyRepository _loyaltyRepository;
    private readonly IValidator<CreateLoyaltyPlanCommand> _validator;

    public CreateLoyaltyPlanCommandHandler(
        ILoyaltyRepository loyaltyRepository,
        IValidator<CreateLoyaltyPlanCommand> validator)
    {
        _loyaltyRepository = loyaltyRepository;
        _validator = validator;
    }

    public async Task<AdminLoyaltyPlanDto> Handle(CreateLoyaltyPlanCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var plan = new LoyaltyPlan
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            PuntosPorCita = request.PuntosXCita,
            PuntosPorDolar = request.PuntosXDolar,
            Activo = request.Activo,
            Levels = request.Niveles
                .Select((nivel, index) => new LoyaltyLevel
                {
                    Id = Guid.NewGuid(),
                    Nombre = nivel.Nombre.Trim(),
                    MinPoints = nivel.PuntosMinimos,
                    MaxPoints = nivel.PuntosMaximos,
                    Orden = index + 1
                })
                .ToList()
        };

        await _loyaltyRepository.AddPlanAsync(plan, cancellationToken);

        var created = await _loyaltyRepository.GetPlanByIdAsync(plan.Id, cancellationToken)
            ?? throw new InvalidOperationException("No fue posible cargar el plan creado.");

        return AdminLoyaltyPlanDto.FromDomain(created);
    }
}

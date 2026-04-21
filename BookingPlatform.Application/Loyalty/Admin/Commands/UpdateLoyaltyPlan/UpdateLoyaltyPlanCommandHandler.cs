using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Loyalty.Admin.DTOs;
using BookingPlatform.Domain.Entities;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Admin.Commands.UpdateLoyaltyPlan;

public class UpdateLoyaltyPlanCommandHandler : IRequestHandler<UpdateLoyaltyPlanCommand, AdminLoyaltyPlanDto>
{
    private readonly ILoyaltyRepository _loyaltyRepository;
    private readonly IValidator<UpdateLoyaltyPlanCommand> _validator;

    public UpdateLoyaltyPlanCommandHandler(
        ILoyaltyRepository loyaltyRepository,
        IValidator<UpdateLoyaltyPlanCommand> validator)
    {
        _loyaltyRepository = loyaltyRepository;
        _validator = validator;
    }

    public async Task<AdminLoyaltyPlanDto> Handle(UpdateLoyaltyPlanCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var plan = await _loyaltyRepository.GetPlanByIdAsync(request.PlanId, cancellationToken)
            ?? throw new KeyNotFoundException("Plan de lealtad no encontrado.");

        plan.Nombre = request.Nombre.Trim();
        plan.PuntosPorCita = request.PuntosXCita;
        plan.PuntosPorDolar = request.PuntosXDolar;
        plan.Activo = request.Activo;
        plan.Levels = request.Niveles
            .Select((nivel, index) => new LoyaltyLevel
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                Nombre = nivel.Nombre.Trim(),
                MinPoints = nivel.PuntosMinimos,
                MaxPoints = nivel.PuntosMaximos,
                Orden = index + 1
            })
            .ToList();

        await _loyaltyRepository.UpdatePlanAsync(plan, cancellationToken);

        var updated = await _loyaltyRepository.GetPlanByIdAsync(plan.Id, cancellationToken)
            ?? throw new InvalidOperationException("No fue posible cargar el plan actualizado.");

        return AdminLoyaltyPlanDto.FromDomain(updated);
    }
}

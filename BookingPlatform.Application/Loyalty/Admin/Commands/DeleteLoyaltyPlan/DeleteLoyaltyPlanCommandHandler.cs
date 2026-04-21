using BookingPlatform.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Admin.Commands.DeleteLoyaltyPlan;

public class DeleteLoyaltyPlanCommandHandler : IRequestHandler<DeleteLoyaltyPlanCommand, Unit>
{
    private readonly ILoyaltyRepository _loyaltyRepository;
    private readonly IValidator<DeleteLoyaltyPlanCommand> _validator;

    public DeleteLoyaltyPlanCommandHandler(
        ILoyaltyRepository loyaltyRepository,
        IValidator<DeleteLoyaltyPlanCommand> validator)
    {
        _loyaltyRepository = loyaltyRepository;
        _validator = validator;
    }

    public async Task<Unit> Handle(DeleteLoyaltyPlanCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var plan = await _loyaltyRepository.GetPlanByIdAsync(request.PlanId, cancellationToken)
            ?? throw new KeyNotFoundException("Plan de lealtad no encontrado.");

        await _loyaltyRepository.DeletePlanAsync(plan, cancellationToken);

        return Unit.Value;
    }
}

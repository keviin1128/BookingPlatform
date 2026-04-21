using FluentValidation;

namespace BookingPlatform.Application.Loyalty.Admin.Commands.DeleteLoyaltyPlan;

public class DeleteLoyaltyPlanCommandValidator : AbstractValidator<DeleteLoyaltyPlanCommand>
{
    public DeleteLoyaltyPlanCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("El plan de lealtad es obligatorio.");
    }
}
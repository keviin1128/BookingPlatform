using BookingPlatform.Application.Loyalty.Admin.DTOs;
using FluentValidation;

namespace BookingPlatform.Application.Loyalty.Admin.Commands.UpdateLoyaltyPlan;

public class UpdateLoyaltyPlanCommandValidator : AbstractValidator<UpdateLoyaltyPlanCommand>
{
    public UpdateLoyaltyPlanCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del plan es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.PuntosXCita)
            .GreaterThanOrEqualTo(0).WithMessage("Los puntos por cita no pueden ser negativos.");

        RuleFor(x => x.PuntosXDolar)
            .GreaterThanOrEqualTo(0).WithMessage("Los puntos por dolar no pueden ser negativos.");

        RuleFor(x => x.Niveles)
            .NotNull().WithMessage("Los niveles son obligatorios.")
            .Must(levels => levels.Count > 0).WithMessage("Debes definir al menos un nivel.");

        RuleForEach(x => x.Niveles)
            .SetValidator(new LoyaltyPlanLevelInputDtoValidator());
    }

    private sealed class LoyaltyPlanLevelInputDtoValidator : AbstractValidator<LoyaltyPlanLevelInputDto>
    {
        public LoyaltyPlanLevelInputDtoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del nivel es obligatorio.")
                .MaximumLength(200);

            RuleFor(x => x.PuntosMinimos)
                .GreaterThanOrEqualTo(0).WithMessage("Los puntos minimos no pueden ser negativos.");

            RuleFor(x => x.PuntosMaximos)
                .GreaterThanOrEqualTo(0)
                .When(x => x.PuntosMaximos.HasValue)
                .WithMessage("Los puntos maximos no pueden ser negativos.");

            RuleFor(x => x)
                .Must(level => !level.PuntosMaximos.HasValue || level.PuntosMaximos.Value >= level.PuntosMinimos)
                .WithMessage("El maximo de puntos debe ser mayor o igual al minimo.");
        }
    }
}

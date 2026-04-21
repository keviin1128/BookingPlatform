using FluentValidation;

namespace BookingPlatform.Application.Services.Commands.CreateService;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripcion es obligatoria.")
            .MaximumLength(1000);

        RuleFor(x => x.Duracion)
            .GreaterThan(0).WithMessage("La duracion debe ser mayor a 0.");

        RuleFor(x => x.Precio)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");
    }
}

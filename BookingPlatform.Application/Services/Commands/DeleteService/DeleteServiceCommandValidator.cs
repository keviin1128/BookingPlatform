using FluentValidation;

namespace BookingPlatform.Application.Services.Commands.DeleteService;

public class DeleteServiceCommandValidator : AbstractValidator<DeleteServiceCommand>
{
    public DeleteServiceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El servicio es obligatorio.");
    }
}
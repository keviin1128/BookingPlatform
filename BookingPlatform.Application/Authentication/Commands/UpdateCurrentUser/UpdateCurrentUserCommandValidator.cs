using BookingPlatform.Application.Common.Utilities;
using FluentValidation;

namespace BookingPlatform.Application.Authentication.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserCommandValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato valido.");

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El telefono es obligatorio.")
            .Must(phone => !string.IsNullOrWhiteSpace(PhoneNumberNormalizer.Normalize(phone)))
            .WithMessage("El telefono es obligatorio.");
    }
}
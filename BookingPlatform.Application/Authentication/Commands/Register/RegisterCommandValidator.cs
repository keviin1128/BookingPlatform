using BookingPlatform.Application.Common.Utilities;
using FluentValidation;

namespace BookingPlatform.Application.Authentication.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El telefono es obligatorio.")
            .Must(phone => !string.IsNullOrWhiteSpace(PhoneNumberNormalizer.Normalize(phone)))
            .WithMessage("El telefono es obligatorio.");

        RuleFor(x => x.Nombre)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Nombre));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no tiene un formato valido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
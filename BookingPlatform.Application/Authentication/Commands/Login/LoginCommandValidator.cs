using BookingPlatform.Application.Common.Utilities;
using FluentValidation;

namespace BookingPlatform.Application.Authentication.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El telefono es obligatorio.")
            .Must(phone => !string.IsNullOrWhiteSpace(PhoneNumberNormalizer.Normalize(phone)))
            .WithMessage("El telefono es obligatorio.");
    }
}
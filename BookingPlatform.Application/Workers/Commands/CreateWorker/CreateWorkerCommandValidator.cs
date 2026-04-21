using BookingPlatform.Application.Common.Utilities;
using FluentValidation;

namespace BookingPlatform.Application.Workers.Commands.CreateWorker;

public class CreateWorkerCommandValidator : AbstractValidator<CreateWorkerCommand>
{
    public CreateWorkerCommandValidator()
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

        RuleFor(x => x.Especialidad)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Especialidad));

        RuleForEach(x => x.Agenda!)
            .Must(entry => entry.Key >= 0 && entry.Key <= 6)
            .WithMessage("El dia de agenda debe estar entre 0 y 6.")
            .When(x => x.Agenda is not null);

        RuleForEach(x => x.Agenda!.Values)
            .NotNull()
            .When(x => x.Agenda is not null);
    }
}

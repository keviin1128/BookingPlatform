using BookingPlatform.Application.Common.Utilities;
using FluentValidation;

namespace BookingPlatform.Application.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.ServicioId)
            .NotEmpty().WithMessage("El servicio es obligatorio.");

        RuleFor(x => x.TrabajadorId)
            .NotEmpty().WithMessage("El trabajador es obligatorio.");

        RuleFor(x => x.Fecha)
            .Must(date => date != default)
            .WithMessage("La fecha es obligatoria.");

        RuleFor(x => x.Hora)
            .NotEmpty().WithMessage("La hora es obligatoria.")
            .Must(BeValidHour)
            .WithMessage("La hora debe tener formato HH:mm.");

        RuleFor(x => x.ClienteNombre)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio para reservas de invitado.")
            .When(x => !x.UsuarioAutenticadoId.HasValue);

        RuleFor(x => x.ClienteTelefono)
            .NotEmpty().WithMessage("El telefono del cliente es obligatorio para reservas de invitado.")
            .Must(phone => !string.IsNullOrWhiteSpace(PhoneNumberNormalizer.Normalize(phone)))
            .WithMessage("El telefono del cliente es obligatorio para reservas de invitado.")
            .When(x => !x.UsuarioAutenticadoId.HasValue);

        RuleFor(x => x.ClienteEmail)
            .EmailAddress().WithMessage("El email del cliente no tiene un formato valido.")
            .When(x => !string.IsNullOrWhiteSpace(x.ClienteEmail));
    }

    private static bool BeValidHour(string value)
    {
        return TimeOnly.TryParseExact(value, "HH:mm", out _);
    }
}

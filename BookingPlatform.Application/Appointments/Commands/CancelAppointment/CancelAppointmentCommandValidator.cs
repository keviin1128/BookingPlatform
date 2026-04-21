using BookingPlatform.Domain.Enums;
using FluentValidation;

namespace BookingPlatform.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("La cita es obligatoria.");

        RuleFor(x => x.RequesterUserId)
            .NotEmpty().WithMessage("El usuario solicitante es obligatorio.");

        RuleFor(x => x.RequesterRole)
            .Must(role => role == Role.Admin || role == Role.Customer)
            .WithMessage("No tienes permisos para cancelar esta cita.");
    }
}
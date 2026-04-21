using BookingPlatform.Domain.Enums;
using FluentValidation;

namespace BookingPlatform.Application.Appointments.Commands.CompleteAppointment;

public class CompleteAppointmentCommandValidator : AbstractValidator<CompleteAppointmentCommand>
{
    public CompleteAppointmentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("La cita es obligatoria.");

        RuleFor(x => x.RequesterUserId)
            .NotEmpty().WithMessage("El usuario solicitante es obligatorio.");

        RuleFor(x => x.RequesterRole)
            .Must(role => role == Role.Admin || role == Role.Worker)
            .WithMessage("No tienes permisos para completar esta cita.");
    }
}
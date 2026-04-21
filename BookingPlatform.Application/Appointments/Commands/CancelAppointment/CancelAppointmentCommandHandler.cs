using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Unit>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IValidator<CancelAppointmentCommand> _validator;

    public CancelAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IValidator<CancelAppointmentCommand> validator)
    {
        _appointmentRepository = appointmentRepository;
        _validator = validator;
    }

    public async Task<Unit> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var appointment = await _appointmentRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Cita no encontrada.");

        if (appointment.Estado is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("La cita ya no puede cancelarse.");
        }

        if (request.RequesterRole == Role.Customer && appointment.ClienteId != request.RequesterUserId)
        {
            throw new UnauthorizedAccessException("No tienes permisos para cancelar esta cita.");
        }

        if (request.RequesterRole != Role.Admin && request.RequesterRole != Role.Customer)
        {
            throw new UnauthorizedAccessException("No tienes permisos para cancelar esta cita.");
        }

        appointment.Estado = AppointmentStatus.Cancelled;

        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

        return Unit.Value;
    }
}

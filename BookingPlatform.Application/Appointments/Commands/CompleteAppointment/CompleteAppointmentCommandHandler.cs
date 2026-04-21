using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Appointments.Commands.CompleteAppointment;

public class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly ILoyaltyRepository? _loyaltyRepository;
    private readonly IValidator<CompleteAppointmentCommand> _validator;

    public CompleteAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IWorkerRepository workerRepository,
        IValidator<CompleteAppointmentCommand> validator,
        ILoyaltyRepository? loyaltyRepository = null)
    {
        _appointmentRepository = appointmentRepository;
        _workerRepository = workerRepository;
        _validator = validator;
        _loyaltyRepository = loyaltyRepository;
    }

    public async Task<AppointmentDto> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var appointment = await _appointmentRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Cita no encontrada.");

        if (appointment.Estado == AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("No se puede completar una cita cancelada.");
        }

        if (appointment.Estado == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("La cita ya esta completada.");
        }

        if (request.RequesterRole == Role.Worker)
        {
            var worker = await _workerRepository.GetByUserIdAsync(request.RequesterUserId, cancellationToken)
                ?? throw new UnauthorizedAccessException("No tienes permisos para completar esta cita.");

            if (appointment.TrabajadorId != worker.Id)
            {
                throw new UnauthorizedAccessException("No tienes permisos para completar esta cita.");
            }
        }
        else if (request.RequesterRole != Role.Admin)
        {
            throw new UnauthorizedAccessException("No tienes permisos para completar esta cita.");
        }

        appointment.Estado = AppointmentStatus.Completed;
        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

        if (_loyaltyRepository is not null && appointment.ClienteRegistrado && appointment.ClienteId.HasValue)
        {
            var plan = await _loyaltyRepository.GetOrCreateActivePlanAsync(cancellationToken);
            var account = await _loyaltyRepository.GetOrCreateAccountAsync(appointment.ClienteId.Value, cancellationToken);

            account.Points += Math.Max(0, plan.PuntosPorCita);
            account.UpdatedAt = DateTime.UtcNow;

            await _loyaltyRepository.UpdateAccountAsync(account, cancellationToken);
        }

        return AppointmentDto.FromDomain(appointment);
    }
}

using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Common.Utilities;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateAppointmentCommand> _validator;

    public CreateAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IServiceRepository serviceRepository,
        IWorkerRepository workerRepository,
        IUserRepository userRepository,
        IValidator<CreateAppointmentCommand> validator)
    {
        _appointmentRepository = appointmentRepository;
        _serviceRepository = serviceRepository;
        _workerRepository = workerRepository;
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var service = await _serviceRepository.GetByIdAsync(request.ServicioId, cancellationToken)
            ?? throw new KeyNotFoundException("Servicio no encontrado.");

        var worker = await _workerRepository.GetByIdAsync(request.TrabajadorId, cancellationToken)
            ?? throw new KeyNotFoundException("Trabajador no encontrado.");

        if (!worker.WorkerServices.Any(ws => ws.ServiceId == request.ServicioId))
        {
            throw new InvalidOperationException("El trabajador no ofrece el servicio seleccionado.");
        }

        if (!TimeOnly.TryParseExact(request.Hora, "HH:mm", out var startTime))
        {
            throw new InvalidOperationException("La hora debe tener formato HH:mm.");
        }

        var duration = service.Duracion;
        var endTime = startTime.AddMinutes(duration);

        var dayOfWeek = (int)request.Fecha.DayOfWeek;
        var scheduleForDay = worker.Agenda
            .Where(x => x.DayOfWeek == dayOfWeek)
            .Select(x => new
            {
                Start = TimeOnly.ParseExact(x.StartTime, "HH:mm"),
                End = TimeOnly.ParseExact(x.EndTime, "HH:mm")
            })
            .ToList();

        var fitsSchedule = scheduleForDay.Any(range => startTime >= range.Start && endTime <= range.End);
        if (!fitsSchedule)
        {
            throw new InvalidOperationException("La hora seleccionada no esta disponible en la agenda del trabajador.");
        }

        var existingAppointments = await _appointmentRepository
            .GetByWorkerAndDateAsync(worker.Id, request.Fecha, cancellationToken);

        var overlaps = existingAppointments.Any(existing =>
            existing.Estado != AppointmentStatus.Cancelled &&
            startTime < existing.Hora.AddMinutes(existing.Duracion) &&
            existing.Hora < endTime);

        if (overlaps)
        {
            throw new InvalidOperationException("La hora seleccionada no esta disponible.");
        }

        Guid? clientId = null;
        string clientName;
        string? clientEmail;
        string? clientPhone;
        bool isRegisteredClient;

        if (request.UsuarioAutenticadoId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(request.UsuarioAutenticadoId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Usuario autenticado no encontrado.");

            clientId = user.Id;
            clientName = user.Nombre ?? string.Empty;
            clientEmail = user.Email;
            clientPhone = user.Telefono;
            isRegisteredClient = true;
        }
        else
        {
            clientName = request.ClienteNombre!.Trim();
            clientEmail = string.IsNullOrWhiteSpace(request.ClienteEmail) ? null : request.ClienteEmail.Trim();
            clientPhone = PhoneNumberNormalizer.Normalize(request.ClienteTelefono!);
            isRegisteredClient = false;
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Servicio = service.Nombre,
            ServicioId = service.Id,
            Fecha = request.Fecha,
            Hora = startTime,
            Estado = AppointmentStatus.Pending,
            Precio = service.Precio,
            Duracion = duration,
            TrabajadorId = worker.Id,
            TrabajadorNombre = worker.User.Nombre ?? request.TrabajadorNombre,
            ClienteId = clientId,
            ClienteNombre = clientName,
            ClienteEmail = clientEmail,
            ClienteTelefono = clientPhone,
            ClienteRegistrado = isRegisteredClient
        };

        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        return AppointmentDto.FromDomain(appointment);
    }
}

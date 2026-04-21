using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Common.Utilities;
using BookingPlatform.Application.Workers.DTOs;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Workers.Commands.CreateWorker;

public class CreateWorkerCommandHandler : IRequestHandler<CreateWorkerCommand, WorkerDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IValidator<CreateWorkerCommand> _validator;

    public CreateWorkerCommandHandler(
        IUserRepository userRepository,
        IWorkerRepository workerRepository,
        IServiceRepository serviceRepository,
        IValidator<CreateWorkerCommand> validator)
    {
        _userRepository = userRepository;
        _workerRepository = workerRepository;
        _serviceRepository = serviceRepository;
        _validator = validator;
    }

    public async Task<WorkerDto> Handle(CreateWorkerCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedPhone = PhoneNumberNormalizer.Normalize(request.Telefono);
        var user = await _userRepository.GetByPhoneAsync(normalizedPhone, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Nombre = string.IsNullOrWhiteSpace(request.Nombre) ? string.Empty : request.Nombre.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email) ? string.Empty : request.Email.Trim(),
                Telefono = normalizedPhone,
                Role = Role.Worker
            };

            await _userRepository.AddAsync(user, cancellationToken);
        }
        else
        {
            var existingWorker = await _workerRepository.ExistsByUserIdAsync(user.Id, cancellationToken);
            if (existingWorker)
            {
                throw new InvalidOperationException("El trabajador ya existe para este telefono.");
            }

            user.Role = Role.Worker;
            if (!string.IsNullOrWhiteSpace(request.Nombre))
            {
                user.Nombre = request.Nombre.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user.Email = request.Email.Trim();
            }

            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        var serviceIds = (request.Servicios ?? Array.Empty<Guid>())
            .Distinct()
            .ToList();

        foreach (var serviceId in serviceIds)
        {
            if (!await _serviceRepository.ExistsAsync(serviceId, cancellationToken))
            {
                throw new KeyNotFoundException("Servicio no encontrado.");
            }
        }

        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Especialidad = string.IsNullOrWhiteSpace(request.Especialidad) ? string.Empty : request.Especialidad.Trim(),
            HorarioResumen = BuildHorarioResumen(request.Agenda),
            WorkerServices = serviceIds
                .Select(serviceId => new WorkerService
                {
                    WorkerId = Guid.Empty,
                    ServiceId = serviceId
                })
                .ToList(),
            Agenda = BuildAgenda(request.Agenda)
        };

        foreach (var workerService in worker.WorkerServices)
        {
            workerService.WorkerId = worker.Id;
        }

        foreach (var schedule in worker.Agenda)
        {
            schedule.WorkerId = worker.Id;
        }

        await _workerRepository.AddAsync(worker, cancellationToken);

        var created = await _workerRepository.GetByIdAsync(worker.Id, cancellationToken)
            ?? throw new InvalidOperationException("No fue posible cargar el trabajador creado.");

        return WorkerDto.FromDomain(created, includeAgenda: true);
    }

    private static List<WorkerScheduleEntry> BuildAgenda(Dictionary<int, IReadOnlyList<ScheduleRangeDto>>? agenda)
    {
        var scheduleEntries = new List<WorkerScheduleEntry>();

        if (agenda is null)
        {
            return scheduleEntries;
        }

        foreach (var day in agenda)
        {
            foreach (var range in day.Value)
            {
                ValidateRange(range);
                scheduleEntries.Add(new WorkerScheduleEntry
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = day.Key,
                    StartTime = range.Inicio,
                    EndTime = range.Fin
                });
            }
        }

        return scheduleEntries;
    }

    private static string BuildHorarioResumen(Dictionary<int, IReadOnlyList<ScheduleRangeDto>>? agenda)
    {
        if (agenda is null || agenda.Count == 0)
        {
            return string.Empty;
        }

        var parts = agenda
            .OrderBy(x => x.Key)
            .Select(day =>
            {
                var ranges = string.Join(",", day.Value.Select(x => $"{x.Inicio}-{x.Fin}"));
                return $"{day.Key}:{ranges}";
            });

        return string.Join(" | ", parts);
    }

    private static void ValidateRange(ScheduleRangeDto range)
    {
        if (!TimeOnly.TryParse(range.Inicio, out var start))
        {
            throw new InvalidOperationException("Formato de hora inicial invalido en agenda.");
        }

        if (!TimeOnly.TryParse(range.Fin, out var end))
        {
            throw new InvalidOperationException("Formato de hora final invalido en agenda.");
        }

        if (start >= end)
        {
            throw new InvalidOperationException("La hora inicial debe ser menor a la hora final en agenda.");
        }
    }
}

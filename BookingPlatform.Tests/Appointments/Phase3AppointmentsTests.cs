using BookingPlatform.Application.Appointments.Commands.CancelAppointment;
using BookingPlatform.Application.Appointments.Commands.CompleteAppointment;
using BookingPlatform.Application.Appointments.Commands.CreateAppointment;
using BookingPlatform.Application.Appointments.Queries.GetAppointments;
using BookingPlatform.Application.Appointments.Queries.GetAvailableSlots;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Tests.Appointments;

public class Phase3AppointmentsTests
{
    [Fact]
    public async Task CreateAppointment_Guest_WorksWithRequiredFields()
    {
        var fixture = new AppointmentFixture();
        var handler = fixture.CreateHandler();

        var command = new CreateAppointmentCommand
        {
            ServicioId = fixture.Service.Id,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            Hora = "09:00",
            TrabajadorId = fixture.Worker.Id,
            ClienteNombre = "Invitado",
            ClienteTelefono = "555-123-4567"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("pendiente", result.Estado);
        Assert.False(result.ClienteRegistrado);
        Assert.Equal("Invitado", result.ClienteNombre);
        Assert.Equal("5551234567", result.ClienteTelefono);
    }

    [Fact]
    public async Task GetAppointments_Customer_OnlyOwnAppointments()
    {
        var fixture = new AppointmentFixture();
        var queryHandler = fixture.GetAppointmentsHandler();

        var own = fixture.CreateAppointmentForCustomer(fixture.Customer.Id);
        var other = fixture.CreateAppointmentForCustomer(Guid.NewGuid());
        fixture.Appointments.Add(own);
        fixture.Appointments.Add(other);

        var result = await queryHandler.Handle(new GetAppointmentsQuery(Role.Customer, fixture.Customer.Id), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(own.Id, result[0].Id);
    }

    [Fact]
    public async Task CancelAppointment_Completed_Throws()
    {
        var fixture = new AppointmentFixture();
        var completed = fixture.CreateAppointmentForCustomer(fixture.Customer.Id);
        completed.Estado = AppointmentStatus.Completed;
        fixture.Appointments.Add(completed);

        var handler = new CancelAppointmentCommandHandler(fixture.AppointmentRepository, new CancelAppointmentCommandValidator());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new CancelAppointmentCommand
            {
                Id = completed.Id,
                RequesterRole = Role.Customer,
                RequesterUserId = fixture.Customer.Id
            }, CancellationToken.None));
    }

    [Fact]
    public async Task CompleteAppointment_WorkerOnlyOwn_Enforced()
    {
        var fixture = new AppointmentFixture();
        var assignedPending = fixture.CreateAppointmentForCustomer(fixture.Customer.Id);
        fixture.Appointments.Add(assignedPending);

        var anotherPending = fixture.CreateAppointmentForCustomer(fixture.Customer.Id);
        fixture.Appointments.Add(anotherPending);

        var handler = new CompleteAppointmentCommandHandler(
            fixture.AppointmentRepository,
            fixture.WorkerRepository,
            new CompleteAppointmentCommandValidator());

        var completed = await handler.Handle(new CompleteAppointmentCommand
        {
            Id = assignedPending.Id,
            RequesterRole = Role.Worker,
            RequesterUserId = fixture.Worker.UserId
        }, CancellationToken.None);

        Assert.Equal("completada", completed.Estado);

        var otherWorkerUserId = Guid.NewGuid();
        fixture.Workers.Add(new Worker
        {
            Id = Guid.NewGuid(),
            UserId = otherWorkerUserId,
            User = new User { Id = otherWorkerUserId, Telefono = "5550000000", Role = Role.Worker }
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new CompleteAppointmentCommand
            {
                Id = anotherPending.Id,
                RequesterRole = Role.Worker,
                RequesterUserId = otherWorkerUserId
            }, CancellationToken.None));
    }

    [Fact]
    public async Task CancelAppointment_InvalidRole_ThrowsValidationError()
    {
        var fixture = new AppointmentFixture();
        var appointment = fixture.CreateAppointmentForCustomer(fixture.Customer.Id);
        fixture.Appointments.Add(appointment);

        var handler = new CancelAppointmentCommandHandler(fixture.AppointmentRepository, new CancelAppointmentCommandValidator());

        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.Handle(new CancelAppointmentCommand
            {
                Id = appointment.Id,
                RequesterRole = Role.Worker,
                RequesterUserId = fixture.Worker.UserId
            }, CancellationToken.None));

        Assert.Contains("No tienes permisos para cancelar esta cita.", exception.Errors.Select(x => x.ErrorMessage));
    }

    [Fact]
    public async Task GetAvailableSlots_ExcludesOverlaps()
    {
        var fixture = new AppointmentFixture();
        var day = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));
        fixture.Appointments.Add(new Appointment
        {
            Id = Guid.NewGuid(),
            Servicio = fixture.Service.Nombre,
            ServicioId = fixture.Service.Id,
            Fecha = day,
            Hora = new TimeOnly(9, 0),
            Duracion = 60,
            Precio = fixture.Service.Precio,
            TrabajadorId = fixture.Worker.Id,
            TrabajadorNombre = fixture.Worker.User.Nombre ?? string.Empty,
            ClienteNombre = "Cliente",
            ClienteRegistrado = false,
            Estado = AppointmentStatus.Pending
        });

        var handler = new GetAvailableSlotsQueryHandler(
            fixture.AppointmentRepository,
            fixture.ServiceRepository,
            fixture.WorkerRepository);

        var result = await handler.Handle(new GetAvailableSlotsQuery(day, fixture.Worker.Id, fixture.Service.Id, fixture.Service.Duracion), CancellationToken.None);

        Assert.DoesNotContain("09:00", result);
        Assert.Contains("10:00", result);
    }

    private sealed class AppointmentFixture
    {
        public User Customer { get; }
        public Service Service { get; }
        public Worker Worker { get; }

        public List<User> Users { get; } = new();
        public List<Service> Services { get; } = new();
        public List<Worker> Workers { get; } = new();
        public List<Appointment> Appointments { get; } = new();

        public InMemoryAppointmentRepository AppointmentRepository { get; }
        public InMemoryServiceRepository ServiceRepository { get; }
        public InMemoryWorkerRepository WorkerRepository { get; }
        public InMemoryUserRepository UserRepository { get; }

        public AppointmentFixture()
        {
            Customer = new User
            {
                Id = Guid.NewGuid(),
                Nombre = "Cliente",
                Email = "cliente@example.com",
                Telefono = "5551112222",
                Role = Role.Customer
            };

            Service = new Service
            {
                Id = Guid.NewGuid(),
                Nombre = "Corte",
                Descripcion = "Corte clasico",
                Duracion = 60,
                Precio = 25m,
                Activo = true
            };

            var workerUser = new User
            {
                Id = Guid.NewGuid(),
                Nombre = "Trabajador",
                Telefono = "5553334444",
                Role = Role.Worker
            };

            Worker = new Worker
            {
                Id = Guid.NewGuid(),
                UserId = workerUser.Id,
                User = workerUser,
                WorkerServices = new List<WorkerService>
                {
                    new() { WorkerId = Guid.Empty, ServiceId = Service.Id }
                },
                Agenda = BuildDefaultAgenda()
            };

            foreach (var relation in Worker.WorkerServices)
            {
                relation.WorkerId = Worker.Id;
            }

            foreach (var slot in Worker.Agenda)
            {
                slot.WorkerId = Worker.Id;
            }

            Users.AddRange(new[] { Customer, workerUser });
            Services.Add(Service);
            Workers.Add(Worker);

            AppointmentRepository = new InMemoryAppointmentRepository(Appointments);
            ServiceRepository = new InMemoryServiceRepository(Services);
            WorkerRepository = new InMemoryWorkerRepository(Workers);
            UserRepository = new InMemoryUserRepository(Users);
        }

        public CreateAppointmentCommandHandler CreateHandler()
        {
            return new CreateAppointmentCommandHandler(
                AppointmentRepository,
                ServiceRepository,
                WorkerRepository,
                UserRepository,
                new CreateAppointmentCommandValidator());
        }

        public GetAppointmentsQueryHandler GetAppointmentsHandler()
        {
            return new GetAppointmentsQueryHandler(AppointmentRepository, WorkerRepository);
        }

        public Appointment CreateAppointmentForCustomer(Guid customerId)
        {
            return new Appointment
            {
                Id = Guid.NewGuid(),
                Servicio = Service.Nombre,
                ServicioId = Service.Id,
                Fecha = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
                Hora = new TimeOnly(10, 0),
                Estado = AppointmentStatus.Pending,
                Precio = Service.Precio,
                Duracion = Service.Duracion,
                TrabajadorId = Worker.Id,
                TrabajadorNombre = Worker.User.Nombre ?? string.Empty,
                ClienteId = customerId,
                ClienteNombre = customerId == Customer.Id ? Customer.Nombre ?? string.Empty : "Otro",
                ClienteEmail = "x@example.com",
                ClienteTelefono = "5550001111",
                ClienteRegistrado = true
            };
        }

        private static List<WorkerScheduleEntry> BuildDefaultAgenda()
        {
            var day = (int)DateTime.UtcNow.Date.AddDays(1).DayOfWeek;
            return new List<WorkerScheduleEntry>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    DayOfWeek = day,
                    StartTime = "09:00",
                    EndTime = "12:00"
                }
            };
        }
    }

    private sealed class InMemoryAppointmentRepository : IAppointmentRepository
    {
        private readonly List<Appointment> _appointments;

        public InMemoryAppointmentRepository(List<Appointment> appointments)
        {
            _appointments = appointments;
        }

        public Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            _appointments.Add(appointment);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            var index = _appointments.FindIndex(x => x.Id == appointment.Id);
            if (index >= 0)
            {
                _appointments[index] = appointment;
            }

            return Task.CompletedTask;
        }

        public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_appointments.FirstOrDefault(x => x.Id == id));
        }

        public Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Appointment>)_appointments.ToList());
        }

        public Task<IReadOnlyList<Appointment>> GetByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Appointment>)_appointments.Where(x => x.TrabajadorId == workerId).ToList());
        }

        public Task<IReadOnlyList<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Appointment>)_appointments.Where(x => x.ClienteId == customerId).ToList());
        }

        public Task<IReadOnlyList<Appointment>> GetByWorkerAndDateAsync(Guid workerId, DateOnly date, CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Appointment>)_appointments
                .Where(x => x.TrabajadorId == workerId && x.Fecha == date)
                .ToList());
        }
    }

    private sealed class InMemoryServiceRepository : IServiceRepository
    {
        private readonly List<Service> _services;

        public InMemoryServiceRepository(List<Service> services)
        {
            _services = services;
        }

        public Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<Service>)_services.ToList());
        }

        public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_services.FirstOrDefault(x => x.Id == id));
        }

        public Task AddAsync(Service service, CancellationToken cancellationToken = default)
        {
            _services.Add(service);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Service service, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Service service, CancellationToken cancellationToken = default)
        {
            _services.RemoveAll(x => x.Id == service.Id);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_services.Any(x => x.Id == id));
        }
    }

    private sealed class InMemoryWorkerRepository : IWorkerRepository
    {
        private readonly List<Worker> _workers;

        public InMemoryWorkerRepository(List<Worker> workers)
        {
            _workers = workers;
        }

        public Task AddAsync(Worker worker, CancellationToken cancellationToken = default)
        {
            _workers.Add(worker);
            return Task.CompletedTask;
        }

        public Task<Worker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_workers.FirstOrDefault(x => x.Id == id));
        }

        public Task<Worker?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_workers.FirstOrDefault(x => x.UserId == userId));
        }

        public Task<IReadOnlyList<Worker>> GetAllAsync(string? search, Guid? serviceId, CancellationToken cancellationToken = default)
        {
            var query = _workers.AsQueryable();

            if (serviceId.HasValue)
            {
                query = query.Where(x => x.WorkerServices.Any(ws => ws.ServiceId == serviceId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    (x.User.Nombre ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    x.User.Telefono.Contains(term) ||
                    x.Especialidad.ToLowerInvariant().Contains(term));
            }

            return Task.FromResult((IReadOnlyList<Worker>)query.ToList());
        }

        public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_workers.Any(x => x.UserId == userId));
        }
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users;

        public InMemoryUserRepository(List<User> users)
        {
            _users = users;
        }

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            var index = _users.FindIndex(x => x.Id == user.Id);
            if (index >= 0)
            {
                _users[index] = user;
            }

            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.FirstOrDefault(x => x.Id == id));
        }

        public Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.FirstOrDefault(x => x.Telefono == phone));
        }

        public Task<IReadOnlyList<User>> GetCustomersAsync(string? search, CancellationToken cancellationToken = default)
        {
            var query = _users.Where(x => x.Role == Role.Customer);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    (x.Nombre ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    (x.Email ?? string.Empty).ToLowerInvariant().Contains(term));
            }

            return Task.FromResult((IReadOnlyList<User>)query.ToList());
        }
    }
}

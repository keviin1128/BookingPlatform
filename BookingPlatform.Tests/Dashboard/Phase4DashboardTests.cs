using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Dashboard.Queries.GetDashboard;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Tests.Dashboard;

public class Phase4DashboardTests
{
    [Fact]
    public async Task CustomerDashboard_SummarizesUpcomingAppointmentsAndLoyaltyPoints()
    {
        var fixture = new DashboardFixture();
        fixture.Appointments.AddRange(new[]
        {
            fixture.CreateAppointment(fixture.Customer.Id, AppointmentStatus.Pending, 1, 10, 0),
            fixture.CreateAppointment(fixture.Customer.Id, AppointmentStatus.Cancelled, 1, 11, 0),
            fixture.CreateAppointment(fixture.Customer.Id, AppointmentStatus.Completed, -1, 8, 30)
        });

        var handler = fixture.CreateHandler();

        var response = await handler.Handle(new GetDashboardQuery(Role.Customer, fixture.Customer.Id), CancellationToken.None);

        Assert.NotNull(response.Cliente);
        Assert.Equal("cliente", response.Rol);
        Assert.Equal(1, response.Cliente!.CitasCompletadas);
        Assert.Equal(1, response.Cliente.PuntosLealtad);
        Assert.Equal("Nivel 1", response.Cliente.NivelLealtad);
        Assert.Equal("Nivel 2", response.Cliente.SiguienteNivel);
        Assert.Equal(4, response.Cliente.PuntosParaSiguienteNivel);
        Assert.Single(response.Cliente.ProximasCitas);
        Assert.Equal("10:00", response.Cliente.ProximaCita!.Hora);
    }

    [Fact]
    public async Task WorkerDashboard_SeparatesPendingAndCompletedAppointments()
    {
        var fixture = new DashboardFixture();
        fixture.Appointments.AddRange(new[]
        {
            fixture.CreateWorkerAppointment(AppointmentStatus.Pending, 1, 9, 0),
            fixture.CreateWorkerAppointment(AppointmentStatus.Confirmed, 1, 10, 0),
            fixture.CreateWorkerAppointment(AppointmentStatus.Completed, 1, 11, 0),
            fixture.CreateWorkerAppointment(AppointmentStatus.Cancelled, 1, 12, 0)
        });

        var handler = fixture.CreateHandler();

        var response = await handler.Handle(new GetDashboardQuery(Role.Worker, fixture.WorkerUser.Id), CancellationToken.None);

        Assert.NotNull(response.Trabajador);
        Assert.Equal(2, response.Trabajador!.TotalPendientes);
        Assert.Equal(1, response.Trabajador.TotalCompletadas);
        Assert.Equal(2, response.Trabajador.Pendientes.Count);
        Assert.Single(response.Trabajador.Completadas);
    }

    [Fact]
    public async Task AdminDashboard_AggregatesKpisFromAppointmentsAndWorkers()
    {
        var fixture = new DashboardFixture();
        fixture.Appointments.AddRange(new[]
        {
            fixture.CreateAppointment(fixture.Customer.Id, AppointmentStatus.Pending, 1, 10, 0),
            fixture.CreateAppointment(fixture.Customer.Id, AppointmentStatus.Confirmed, 1, 11, 0),
            fixture.CreateAppointment(fixture.Customer.Id, AppointmentStatus.Completed, 1, 12, 0),
            fixture.CreateAppointment(fixture.Customer.Id, AppointmentStatus.Completed, 1, 13, 0),
            fixture.CreateAppointment(Guid.NewGuid(), AppointmentStatus.Cancelled, 1, 14, 0)
        });

        var handler = fixture.CreateHandler();

        var response = await handler.Handle(new GetDashboardQuery(Role.Admin, fixture.AdminUser.Id), CancellationToken.None);

        Assert.NotNull(response.Admin);
        Assert.Equal(2, response.Admin!.ClientesTotales);
        Assert.Equal(1, response.Admin.TrabajadoresTotales);
        Assert.Equal(5, response.Admin.CitasTotales);
        Assert.Equal(1, response.Admin.CitasPendientes);
        Assert.Equal(1, response.Admin.CitasConfirmadas);
        Assert.Equal(2, response.Admin.CitasCompletadas);
        Assert.Equal(1, response.Admin.CitasCanceladas);
        Assert.Equal("operativo", response.Admin.EstadoOperacional);
        Assert.Equal(5, response.Admin.AgendaGeneral.Count);
    }

    private sealed class DashboardFixture
    {
        public User AdminUser { get; }
        public User Customer { get; }
        public User WorkerUser { get; }
        public Worker Worker { get; }
        public Service Service { get; }

        public List<User> Users { get; } = new();
        public List<Worker> Workers { get; } = new();
        public List<Appointment> Appointments { get; } = new();

        public InMemoryUserRepository UserRepository { get; }
        public InMemoryWorkerRepository WorkerRepository { get; }
        public InMemoryAppointmentRepository AppointmentRepository { get; }

        public DashboardFixture()
        {
            AdminUser = new User
            {
                Id = Guid.NewGuid(),
                Nombre = "Admin",
                Telefono = "5551000000",
                Role = Role.Admin
            };

            Customer = new User
            {
                Id = Guid.NewGuid(),
                Nombre = "Cliente",
                Email = "cliente@example.com",
                Telefono = "5552000000",
                Role = Role.Customer
            };

            WorkerUser = new User
            {
                Id = Guid.NewGuid(),
                Nombre = "Trabajador",
                Telefono = "5553000000",
                Role = Role.Worker
            };

            Service = new Service
            {
                Id = Guid.NewGuid(),
                Nombre = "Corte",
                Descripcion = "Corte clasico",
                Duracion = 60,
                Precio = 20m,
                Activo = true
            };

            Worker = new Worker
            {
                Id = Guid.NewGuid(),
                UserId = WorkerUser.Id,
                User = WorkerUser,
                Especialidad = "Barberia",
                HorarioResumen = "L-V 09:00-18:00",
                WorkerServices = new List<WorkerService>
                {
                    new() { WorkerId = Guid.Empty, ServiceId = Service.Id }
                },
                Agenda = new List<WorkerScheduleEntry>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        WorkerId = Guid.Empty,
                        DayOfWeek = (int)DateTime.UtcNow.AddDays(1).DayOfWeek,
                        StartTime = "09:00",
                        EndTime = "18:00"
                    }
                }
            };

            foreach (var relation in Worker.WorkerServices)
            {
                relation.WorkerId = Worker.Id;
            }

            foreach (var schedule in Worker.Agenda)
            {
                schedule.WorkerId = Worker.Id;
            }

            Users.AddRange(new[] { AdminUser, Customer, WorkerUser });
            Workers.Add(Worker);

            UserRepository = new InMemoryUserRepository(Users);
            WorkerRepository = new InMemoryWorkerRepository(Workers);
            AppointmentRepository = new InMemoryAppointmentRepository(Appointments);
        }

        public GetDashboardQueryHandler CreateHandler()
        {
            return new GetDashboardQueryHandler(UserRepository, AppointmentRepository, WorkerRepository);
        }

        public Appointment CreateAppointment(Guid clientId, AppointmentStatus status, int dayOffset, int hour, int minute)
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(dayOffset));

            return new Appointment
            {
                Id = Guid.NewGuid(),
                Servicio = Service.Nombre,
                ServicioId = Service.Id,
                Fecha = date,
                Hora = new TimeOnly(hour, minute),
                Estado = status,
                Precio = Service.Precio,
                Duracion = Service.Duracion,
                TrabajadorId = Worker.Id,
                TrabajadorNombre = Worker.User.Nombre ?? string.Empty,
                ClienteId = clientId,
                ClienteNombre = clientId == Customer.Id ? Customer.Nombre ?? string.Empty : "Cliente externo",
                ClienteEmail = "cliente@example.com",
                ClienteTelefono = "5552000000",
                ClienteRegistrado = true
            };
        }

        public Appointment CreateWorkerAppointment(AppointmentStatus status, int dayOffset, int hour, int minute)
        {
            return CreateAppointment(Customer.Id, status, dayOffset, hour, minute);
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
            var index = _users.FindIndex(existing => existing.Id == user.Id);
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
            return Task.FromResult((IReadOnlyList<Worker>)_workers.ToList());
        }

        public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_workers.Any(x => x.UserId == userId));
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
            var index = _appointments.FindIndex(existing => existing.Id == appointment.Id);
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
            return Task.FromResult((IReadOnlyList<Appointment>)_appointments.Where(x => x.TrabajadorId == workerId && x.Fecha == date).ToList());
        }
    }
}
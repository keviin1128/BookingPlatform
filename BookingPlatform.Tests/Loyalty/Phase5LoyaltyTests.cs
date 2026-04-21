using BookingPlatform.Application.Appointments.Commands.CompleteAppointment;
using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Loyalty.Commands.RedeemReward;
using BookingPlatform.Application.Loyalty.Queries.GetLoyalty;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Tests.Loyalty;

public class Phase5LoyaltyTests
{
    [Fact]
    public async Task GetLoyaltyQuery_ReturnsCurrentProgressAndRewards()
    {
        var fixture = new LoyaltyFixture(points: 6);
        var handler = new GetLoyaltyQueryHandler(fixture.UserRepository, fixture.LoyaltyRepository);

        var response = await handler.Handle(new GetLoyaltyQuery(fixture.Customer.Id, fixture.Customer.Id, Role.Customer), CancellationToken.None);

        Assert.Equal(fixture.Customer.Id, response.UsuarioId);
        Assert.Equal(6, response.Puntos);
        Assert.Equal("Nivel 2", response.Nivel);
        Assert.Equal("Nivel 3", response.SiguienteNivel);
        Assert.Equal(4, response.PuntosParaSiguienteNivel);
        Assert.Single(response.RecompensasDisponibles);
        Assert.Empty(response.HistorialCanjes);
    }

    [Fact]
    public async Task RedeemRewardCommand_DeductsPointsAndRecordsHistory()
    {
        var fixture = new LoyaltyFixture(points: 10);
        var handler = new RedeemRewardCommandHandler(fixture.UserRepository, fixture.LoyaltyRepository, new RedeemRewardCommandValidator());

        var response = await handler.Handle(new RedeemRewardCommand
        {
            UserId = fixture.Customer.Id,
            RewardId = fixture.Reward.Id,
            RequesterUserId = fixture.Customer.Id,
            RequesterRole = Role.Customer
        }, CancellationToken.None);

        Assert.Equal(5, response.Puntos);
        Assert.Single(response.HistorialCanjes);
        Assert.Equal(fixture.Reward.Id, response.HistorialCanjes[0].RewardId);
        Assert.Equal(1, fixture.Reward.CantidadDisponible);
        Assert.Equal(5, fixture.Account.Points);
    }

    [Fact]
    public async Task CompleteAppointmentCommand_AwardsPointsToRegisteredCustomer()
    {
        var fixture = new LoyaltyFixture(points: 0);
        var handler = new CompleteAppointmentCommandHandler(
            fixture.AppointmentRepository,
            fixture.WorkerRepository,
            new CompleteAppointmentCommandValidator(),
            fixture.LoyaltyRepository);

        var response = await handler.Handle(new CompleteAppointmentCommand
        {
            Id = fixture.Appointment.Id,
            RequesterRole = Role.Admin,
            RequesterUserId = Guid.NewGuid()
        }, CancellationToken.None);

        Assert.Equal("completada", response.Estado);
        Assert.Equal(1, fixture.Account.Points);
    }

    [Fact]
    public async Task RedeemRewardCommand_RejectsWhenPointsAreInsufficient()
    {
        var fixture = new LoyaltyFixture(points: 2);
        var handler = new RedeemRewardCommandHandler(fixture.UserRepository, fixture.LoyaltyRepository, new RedeemRewardCommandValidator());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new RedeemRewardCommand
            {
                UserId = fixture.Customer.Id,
                RewardId = fixture.Reward.Id,
                RequesterUserId = fixture.Customer.Id,
                RequesterRole = Role.Customer
            }, CancellationToken.None));

        Assert.Equal("No tienes puntos suficientes para canjear la recompensa.", exception.Message);
    }

    private sealed class LoyaltyFixture
    {
        public User Customer { get; }
        public User WorkerUser { get; }
        public Worker Worker { get; }
        public Service Service { get; }
        public Appointment Appointment { get; }
        public LoyaltyPlan Plan { get; }
        public LoyaltyReward Reward { get; }
        public LoyaltyAccount Account { get; }

        public InMemoryUserRepository UserRepository { get; }
        public InMemoryWorkerRepository WorkerRepository { get; }
        public InMemoryAppointmentRepository AppointmentRepository { get; }
        public InMemoryLoyaltyRepository LoyaltyRepository { get; }

        public LoyaltyFixture(int points)
        {
            Customer = new User
            {
                Id = Guid.NewGuid(),
                Nombre = "Cliente",
                Email = "cliente@example.com",
                Telefono = "5551112222",
                Role = Role.Customer
            };

            WorkerUser = new User
            {
                Id = Guid.NewGuid(),
                Nombre = "Trabajador",
                Telefono = "5553334444",
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

            Appointment = new Appointment
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
                TrabajadorNombre = WorkerUser.Nombre ?? string.Empty,
                ClienteId = Customer.Id,
                ClienteNombre = Customer.Nombre ?? string.Empty,
                ClienteEmail = Customer.Email,
                ClienteTelefono = Customer.Telefono,
                ClienteRegistrado = true
            };

            Plan = LoyaltyPlan.CreateDefault();
            Reward = Plan.Rewards.First();
            Reward.CantidadDisponible = 2;
            Account = new LoyaltyAccount
            {
                Id = Guid.NewGuid(),
                UserId = Customer.Id,
                PlanId = Plan.Id,
                Points = points,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            UserRepository = new InMemoryUserRepository(new List<User> { Customer, WorkerUser });
            WorkerRepository = new InMemoryWorkerRepository(new List<Worker> { Worker });
            AppointmentRepository = new InMemoryAppointmentRepository(new List<Appointment> { Appointment });
            LoyaltyRepository = new InMemoryLoyaltyRepository(Plan, Reward, Account);
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
            return Task.FromResult((IReadOnlyList<Appointment>)_appointments.Where(x => x.TrabajadorId == workerId && x.Fecha == date).ToList());
        }
    }

    private sealed class InMemoryLoyaltyRepository : ILoyaltyRepository
    {
        private readonly LoyaltyPlan _plan;
        private readonly LoyaltyReward _reward;
        private readonly LoyaltyAccount _account;
        private readonly List<LoyaltyRedemption> _redemptions = new();

        public InMemoryLoyaltyRepository(LoyaltyPlan plan, LoyaltyReward reward, LoyaltyAccount account)
        {
            _plan = plan;
            _reward = reward;
            _account = account;
        }

        public Task<LoyaltyPlan> GetOrCreateActivePlanAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_plan);
        }

        public Task<LoyaltyAccount> GetOrCreateAccountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_account);
        }

        public Task<LoyaltyReward?> GetRewardByIdAsync(Guid rewardId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(rewardId == _reward.Id ? _reward : null);
        }

        public Task<IReadOnlyList<LoyaltyPlan>> GetPlansAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<LoyaltyPlan>)new List<LoyaltyPlan> { _plan });
        }

        public Task<LoyaltyPlan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(planId == _plan.Id ? _plan : null);
        }

        public Task AddPlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task UpdatePlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task DeletePlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<LoyaltyRedemption>> GetRedemptionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<LoyaltyRedemption>)_redemptions.Where(x => x.LoyaltyAccountId == _account.Id).ToList());
        }

        public Task<int> GetTotalAccountsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public Task<int> GetTotalRedemptionsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_redemptions.Count);
        }

        public Task<decimal> GetAveragePointsPerAccountAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((decimal)_account.Points);
        }

        public Task UpdateAccountAsync(LoyaltyAccount account, CancellationToken cancellationToken = default)
        {
            _account.Points = account.Points;
            _account.UpdatedAt = account.UpdatedAt;
            return Task.CompletedTask;
        }

        public Task UpdateRewardAsync(LoyaltyReward reward, CancellationToken cancellationToken = default)
        {
            _reward.CantidadDisponible = reward.CantidadDisponible;
            _reward.Activo = reward.Activo;
            return Task.CompletedTask;
        }

        public Task AddRedemptionAsync(LoyaltyRedemption redemption, CancellationToken cancellationToken = default)
        {
            _redemptions.Add(redemption);
            return Task.CompletedTask;
        }
    }
}
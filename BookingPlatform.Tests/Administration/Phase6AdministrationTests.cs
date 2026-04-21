using BookingPlatform.Application.Admin.Queries.GetCustomers;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Loyalty.Admin.Commands.CreateLoyaltyPlan;
using BookingPlatform.Application.Loyalty.Admin.Commands.DeleteLoyaltyPlan;
using BookingPlatform.Application.Loyalty.Admin.Commands.UpdateLoyaltyPlan;
using BookingPlatform.Application.Loyalty.Admin.DTOs;
using BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyStats;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Tests.Administration;

public class Phase6AdministrationTests
{
    [Fact]
    public async Task GetCustomersQuery_FiltersOnlyCustomersByNameOrEmail()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Ana Cliente", Email = "ana@demo.com", Telefono = "5551110001", Role = Role.Customer },
            new() { Id = Guid.NewGuid(), Nombre = "Beto Cliente", Email = "beto@demo.com", Telefono = "5551110002", Role = Role.Customer },
            new() { Id = Guid.NewGuid(), Nombre = "Wendy Worker", Email = "wendy@demo.com", Telefono = "5551110003", Role = Role.Worker }
        };

        var handler = new GetCustomersQueryHandler(new InMemoryUserRepository(users));

        var result = await handler.Handle(new GetCustomersQuery("ana"), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Ana Cliente", result[0].Nombre);
        Assert.Equal("cliente", result[0].Rol);
    }

    [Fact]
    public async Task LoyaltyPlanCrud_WorksWithCompleteLevels()
    {
        var repository = new InMemoryLoyaltyRepository();
        var createHandler = new CreateLoyaltyPlanCommandHandler(repository, new CreateLoyaltyPlanCommandValidator());
        var updateHandler = new UpdateLoyaltyPlanCommandHandler(repository, new UpdateLoyaltyPlanCommandValidator());
        var deleteHandler = new DeleteLoyaltyPlanCommandHandler(repository, new DeleteLoyaltyPlanCommandValidator());

        var created = await createHandler.Handle(new CreateLoyaltyPlanCommand(
            "Plan Gold",
            "Plan principal",
            2,
            0.5m,
            new List<LoyaltyPlanLevelInputDto>
            {
                new("Bronce", 0, 9),
                new("Plata", 10, 19),
                new("Oro", 20, null)
            },
            true), CancellationToken.None);

        var updated = await updateHandler.Handle(new UpdateLoyaltyPlanCommand
        {
            PlanId = created.Id,
            Nombre = "Plan Gold Plus",
            Descripcion = "Actualizado",
            PuntosXCita = 3,
            PuntosXDolar = 0.75m,
            Niveles = new List<LoyaltyPlanLevelInputDto>
            {
                new("Bronce", 0, 14),
                new("Oro", 15, null)
            },
            Activo = true
        }, CancellationToken.None);

        Assert.Equal("Plan Gold Plus", updated.Nombre);
        Assert.Equal(3, updated.PuntosXCita);
        Assert.Equal(2, updated.Niveles.Count);

        await deleteHandler.Handle(new DeleteLoyaltyPlanCommand { PlanId = created.Id }, CancellationToken.None);

        var plansAfterDelete = await repository.GetPlansAsync(CancellationToken.None);
        Assert.Empty(plansAfterDelete);
    }

    [Fact]
    public async Task GetLoyaltyStatsQuery_ReturnsAggregatedMetrics()
    {
        var repository = new InMemoryLoyaltyRepository();
        var plan = LoyaltyPlan.CreateDefault();
        plan.Activo = true;
        await repository.AddPlanAsync(plan, CancellationToken.None);
        repository.SeedAccounts(2, 8);
        repository.SeedRedemptions(3);

        var handler = new GetLoyaltyStatsQueryHandler(repository);

        var result = await handler.Handle(new GetLoyaltyStatsQuery(), CancellationToken.None);

        Assert.Equal(1, result.PlanesTotales);
        Assert.Equal(1, result.PlanesActivos);
        Assert.Equal(2, result.CuentasLealtadTotales);
        Assert.Equal(3, result.CanjesTotales);
        Assert.Equal(8m, result.PuntosPromedioPorCuenta);
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

    private sealed class InMemoryLoyaltyRepository : ILoyaltyRepository
    {
        private readonly List<LoyaltyPlan> _plans = new();
        private readonly List<LoyaltyAccount> _accounts = new();
        private readonly List<LoyaltyRedemption> _redemptions = new();

        public Task<LoyaltyPlan> GetOrCreateActivePlanAsync(CancellationToken cancellationToken = default)
        {
            var active = _plans.FirstOrDefault(x => x.Activo);
            if (active is not null)
            {
                return Task.FromResult(active);
            }

            var created = LoyaltyPlan.CreateDefault();
            _plans.Add(created);
            return Task.FromResult(created);
        }

        public Task<LoyaltyAccount> GetOrCreateAccountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var existing = _accounts.FirstOrDefault(x => x.UserId == userId);
            if (existing is not null)
            {
                return Task.FromResult(existing);
            }

            var plan = _plans.FirstOrDefault() ?? LoyaltyPlan.CreateDefault();
            if (!_plans.Any(x => x.Id == plan.Id))
            {
                _plans.Add(plan);
            }

            var account = new LoyaltyAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PlanId = plan.Id,
                Points = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _accounts.Add(account);
            return Task.FromResult(account);
        }

        public Task<LoyaltyReward?> GetRewardByIdAsync(Guid rewardId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_plans.SelectMany(x => x.Rewards).FirstOrDefault(x => x.Id == rewardId));
        }

        public Task<IReadOnlyList<LoyaltyPlan>> GetPlansAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((IReadOnlyList<LoyaltyPlan>)_plans.ToList());
        }

        public Task<LoyaltyPlan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_plans.FirstOrDefault(x => x.Id == planId));
        }

        public Task AddPlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default)
        {
            _plans.Add(plan);
            return Task.CompletedTask;
        }

        public Task UpdatePlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default)
        {
            var index = _plans.FindIndex(x => x.Id == plan.Id);
            if (index >= 0)
            {
                _plans[index] = plan;
            }

            return Task.CompletedTask;
        }

        public Task DeletePlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default)
        {
            if (_accounts.Any(x => x.PlanId == plan.Id))
            {
                throw new InvalidOperationException("No se puede eliminar un plan con cuentas de lealtad asociadas.");
            }

            _plans.RemoveAll(x => x.Id == plan.Id);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<LoyaltyRedemption>> GetRedemptionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var accountIds = _accounts.Where(x => x.UserId == userId).Select(x => x.Id).ToHashSet();
            return Task.FromResult((IReadOnlyList<LoyaltyRedemption>)_redemptions.Where(x => accountIds.Contains(x.LoyaltyAccountId)).ToList());
        }

        public Task<int> GetTotalAccountsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_accounts.Count);
        }

        public Task<int> GetTotalRedemptionsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_redemptions.Count);
        }

        public Task<decimal> GetAveragePointsPerAccountAsync(CancellationToken cancellationToken = default)
        {
            if (_accounts.Count == 0)
            {
                return Task.FromResult(0m);
            }

            return Task.FromResult(_accounts.Average(x => (decimal)x.Points));
        }

        public Task UpdateAccountAsync(LoyaltyAccount account, CancellationToken cancellationToken = default)
        {
            var index = _accounts.FindIndex(x => x.Id == account.Id);
            if (index >= 0)
            {
                _accounts[index] = account;
            }

            return Task.CompletedTask;
        }

        public Task UpdateRewardAsync(LoyaltyReward reward, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task AddRedemptionAsync(LoyaltyRedemption redemption, CancellationToken cancellationToken = default)
        {
            _redemptions.Add(redemption);
            return Task.CompletedTask;
        }

        public void SeedAccounts(int count, int pointsPerAccount)
        {
            var planId = _plans.First().Id;
            for (var i = 0; i < count; i++)
            {
                _accounts.Add(new LoyaltyAccount
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    PlanId = planId,
                    Points = pointsPerAccount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        public void SeedRedemptions(int count)
        {
            var accountId = _accounts.First().Id;
            for (var i = 0; i < count; i++)
            {
                _redemptions.Add(new LoyaltyRedemption
                {
                    Id = Guid.NewGuid(),
                    LoyaltyAccountId = accountId,
                    RewardId = Guid.NewGuid(),
                    RewardName = "Recompensa",
                    PointsSpent = 3,
                    RedeemedAt = DateTime.UtcNow
                });
            }
        }
    }
}

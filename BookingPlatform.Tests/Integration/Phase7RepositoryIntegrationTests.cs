using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Infrastructure.Data;
using BookingPlatform.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatform.Tests.Integration;

public class Phase7RepositoryIntegrationTests
{
    [Fact]
    public async Task UserRepository_EnforcesUniquePhoneConstraint()
    {
        await using var context = await CreateContextAsync();
        IUserRepository repository = new UserRepository(context);

        await repository.AddAsync(new User
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente Uno",
            Email = "uno@example.com",
            Telefono = "5551234567",
            Role = Role.Customer
        }, CancellationToken.None);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            repository.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                Nombre = "Cliente Dos",
                Email = "dos@example.com",
                Telefono = "5551234567",
                Role = Role.Customer
            }, CancellationToken.None));
    }

    [Fact]
    public async Task AppointmentRepository_FiltersByWorkerAndDate()
    {
        await using var context = await CreateContextAsync();
        var seed = await SeedAppointmentsGraphAsync(context);

        IAppointmentRepository repository = new AppointmentRepository(context);

        var result = await repository.GetByWorkerAndDateAsync(seed.Worker.Id, seed.TargetDate, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(seed.ExpectedAppointmentId, result[0].Id);
    }

    [Fact]
    public async Task LoyaltyRepository_PersistsAccountAndRedemption()
    {
        await using var context = await CreateContextAsync();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nombre = "Cliente Loyalty",
            Email = "loyalty@example.com",
            Telefono = "5550001111",
            Role = Role.Customer
        };

        await context.Users.AddAsync(user, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        ILoyaltyRepository repository = new LoyaltyRepository(context);

        var plan = await repository.GetOrCreateActivePlanAsync(CancellationToken.None);
        var reward = plan.Rewards.First();
        var account = await repository.GetOrCreateAccountAsync(user.Id, CancellationToken.None);

        account.Points = reward.PuntosRequeridos;
        await repository.UpdateAccountAsync(account, CancellationToken.None);

        await repository.AddRedemptionAsync(new LoyaltyRedemption
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = account.Id,
            RewardId = reward.Id,
            RewardName = reward.Nombre,
            PointsSpent = reward.PuntosRequeridos,
            RedeemedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var redemptions = await repository.GetRedemptionsByUserIdAsync(user.Id, CancellationToken.None);

        Assert.Single(redemptions);
        Assert.Equal(reward.Id, redemptions[0].RewardId);
    }

    private static async Task<AppDbContext> CreateContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return context;
    }

    private static async Task<(Worker Worker, DateOnly TargetDate, Guid ExpectedAppointmentId)> SeedAppointmentsGraphAsync(AppDbContext context)
    {
        var workerUser = new User
        {
            Id = Guid.NewGuid(),
            Nombre = "Worker",
            Email = "worker@test.local",
            Telefono = "5551110001",
            Role = Role.Worker
        };

        var customer = new User
        {
            Id = Guid.NewGuid(),
            Nombre = "Customer",
            Email = "customer@test.local",
            Telefono = "5551110002",
            Role = Role.Customer
        };

        var service = new Service
        {
            Id = Guid.NewGuid(),
            Nombre = "Corte",
            Descripcion = "Corte base",
            Duracion = 60,
            Precio = 25m,
            Activo = true
        };

        var worker = new Worker
        {
            Id = Guid.NewGuid(),
            UserId = workerUser.Id,
            User = workerUser,
            Especialidad = "Corte",
            HorarioResumen = "09:00-18:00"
        };

        var targetDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));
        var expectedAppointmentId = Guid.NewGuid();

        var expectedAppointment = new Appointment
        {
            Id = expectedAppointmentId,
            Servicio = service.Nombre,
            ServicioId = service.Id,
            Fecha = targetDate,
            Hora = new TimeOnly(9, 0),
            Estado = AppointmentStatus.Pending,
            Precio = service.Precio,
            Duracion = service.Duracion,
            TrabajadorId = worker.Id,
            TrabajadorNombre = workerUser.Nombre ?? string.Empty,
            ClienteId = customer.Id,
            ClienteNombre = customer.Nombre ?? string.Empty,
            ClienteEmail = customer.Email,
            ClienteTelefono = customer.Telefono,
            ClienteRegistrado = true
        };

        var otherAppointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Servicio = service.Nombre,
            ServicioId = service.Id,
            Fecha = targetDate.AddDays(1),
            Hora = new TimeOnly(9, 0),
            Estado = AppointmentStatus.Pending,
            Precio = service.Precio,
            Duracion = service.Duracion,
            TrabajadorId = worker.Id,
            TrabajadorNombre = workerUser.Nombre ?? string.Empty,
            ClienteId = customer.Id,
            ClienteNombre = customer.Nombre ?? string.Empty,
            ClienteEmail = customer.Email,
            ClienteTelefono = customer.Telefono,
            ClienteRegistrado = true
        };

        await context.Users.AddRangeAsync(new[] { workerUser, customer }, CancellationToken.None);
        await context.Services.AddAsync(service, CancellationToken.None);
        await context.Workers.AddAsync(worker, CancellationToken.None);
        await context.Appointments.AddRangeAsync(new[] { expectedAppointment, otherAppointment }, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        return (worker, targetDate, expectedAppointmentId);
    }
}

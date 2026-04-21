using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingPlatform.Infrastructure.Data;

public static class AppDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var hasDefinedMigrations = dbContext.Database.GetMigrations().Any();
        if (hasDefinedMigrations)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        await SeedAsync(dbContext, cancellationToken);
    }

    private static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var adminUser = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Nombre = "Admin Demo",
            Email = "admin@bookingplatform.local",
            Telefono = "5551000001",
            Role = Role.Admin,
            CreatedAt = DateTime.UtcNow
        };

        var customerUser = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Nombre = "Cliente Demo",
            Email = "cliente@bookingplatform.local",
            Telefono = "5551000002",
            Role = Role.Customer,
            CreatedAt = DateTime.UtcNow
        };

        var workerUser = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Nombre = "Trabajador Demo",
            Email = "worker@bookingplatform.local",
            Telefono = "5551000003",
            Role = Role.Worker,
            CreatedAt = DateTime.UtcNow
        };

        var baseService = new Service
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Nombre = "Corte Clasico",
            Descripcion = "Servicio base para pruebas funcionales.",
            Duracion = 60,
            Precio = 20m,
            Activo = true,
            CreatedAt = DateTime.UtcNow
        };

        var worker = new Worker
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            UserId = workerUser.Id,
            User = workerUser,
            Especialidad = "Corte",
            HorarioResumen = "Lun-Vie 09:00-18:00",
            CreatedAt = DateTime.UtcNow
        };

        var workerService = new WorkerService
        {
            WorkerId = worker.Id,
            ServiceId = baseService.Id,
            Worker = worker,
            Service = baseService
        };

        var workerSchedule = new WorkerScheduleEntry
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            WorkerId = worker.Id,
            Worker = worker,
            DayOfWeek = 1,
            StartTime = "09:00",
            EndTime = "18:00"
        };

        var sampleAppointment = new Appointment
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Servicio = baseService.Nombre,
            ServicioId = baseService.Id,
            Fecha = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            Hora = new TimeOnly(10, 0),
            Estado = AppointmentStatus.Pending,
            Precio = baseService.Precio,
            Duracion = baseService.Duracion,
            TrabajadorId = worker.Id,
            TrabajadorNombre = workerUser.Nombre ?? string.Empty,
            ClienteId = customerUser.Id,
            ClienteNombre = customerUser.Nombre ?? string.Empty,
            ClienteEmail = customerUser.Email,
            ClienteTelefono = customerUser.Telefono,
            ClienteRegistrado = true,
            CreatedAt = DateTime.UtcNow
        };

        var plan = LoyaltyPlan.CreateDefault();
        var account = new LoyaltyAccount
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            UserId = customerUser.Id,
            PlanId = plan.Id,
            Points = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await dbContext.Users.AddRangeAsync(new[] { adminUser, customerUser, workerUser }, cancellationToken);
        await dbContext.Services.AddAsync(baseService, cancellationToken);
        await dbContext.Workers.AddAsync(worker, cancellationToken);
        await dbContext.WorkerServices.AddAsync(workerService, cancellationToken);
        await dbContext.WorkerScheduleEntries.AddAsync(workerSchedule, cancellationToken);
        await dbContext.Appointments.AddAsync(sampleAppointment, cancellationToken);

        await dbContext.LoyaltyPlans.AddAsync(plan, cancellationToken);
        await dbContext.LoyaltyAccounts.AddAsync(account, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

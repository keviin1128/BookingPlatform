using BookingPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatform.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<Worker> Workers => Set<Worker>();

    public DbSet<WorkerService> WorkerServices => Set<WorkerService>();

    public DbSet<WorkerScheduleEntry> WorkerScheduleEntries => Set<WorkerScheduleEntry>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<LoyaltyPlan> LoyaltyPlans => Set<LoyaltyPlan>();

    public DbSet<LoyaltyLevel> LoyaltyLevels => Set<LoyaltyLevel>();

    public DbSet<LoyaltyReward> LoyaltyRewards => Set<LoyaltyReward>();

    public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();

    public DbSet<LoyaltyRedemption> LoyaltyRedemptions => Set<LoyaltyRedemption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Telefono).IsRequired().HasMaxLength(30);
            entity.Property(x => x.Nombre).HasMaxLength(200);
            entity.Property(x => x.Email).HasMaxLength(320);
            entity.HasIndex(x => x.Telefono).IsUnique();
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Descripcion).IsRequired().HasMaxLength(1000);
            entity.Property(x => x.Duracion).IsRequired();
            entity.Property(x => x.Precio).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(x => x.Activo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Especialidad).HasMaxLength(200);
            entity.Property(x => x.HorarioResumen).HasMaxLength(1000);
            entity.HasIndex(x => x.UserId).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkerService>(entity =>
        {
            entity.HasKey(x => new { x.WorkerId, x.ServiceId });

            entity.HasOne(x => x.Worker)
                .WithMany(x => x.WorkerServices)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Service)
                .WithMany(x => x.WorkerServices)
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkerScheduleEntry>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DayOfWeek).IsRequired();
            entity.Property(x => x.StartTime).IsRequired().HasMaxLength(5);
            entity.Property(x => x.EndTime).IsRequired().HasMaxLength(5);

            entity.HasOne(x => x.Worker)
                .WithMany(x => x.Agenda)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Servicio).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Fecha).HasColumnType("date").IsRequired();
            entity.Property(x => x.Hora).HasColumnType("time").IsRequired();
            entity.Property(x => x.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(x => x.Precio).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(x => x.Duracion).IsRequired();
            entity.Property(x => x.TrabajadorNombre).IsRequired().HasMaxLength(200);
            entity.Property(x => x.ClienteNombre).IsRequired().HasMaxLength(200);
            entity.Property(x => x.ClienteEmail).HasMaxLength(320);
            entity.Property(x => x.ClienteTelefono).HasMaxLength(30);
            entity.Property(x => x.ClienteRegistrado).IsRequired();

            entity.HasIndex(x => new { x.TrabajadorId, x.Fecha, x.Hora });
            entity.HasIndex(x => x.ClienteId);

            entity.HasOne<Service>()
                .WithMany()
                .HasForeignKey(x => x.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Worker>()
                .WithMany()
                .HasForeignKey(x => x.TrabajadorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LoyaltyPlan>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(x => x.PuntosPorCita).IsRequired();
            entity.Property(x => x.PuntosPorDolar).HasColumnType("numeric(18,2)").IsRequired();
            entity.Property(x => x.Activo).IsRequired();
        });

        modelBuilder.Entity<LoyaltyLevel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(x => x.MinPoints).IsRequired();
            entity.Property(x => x.MaxPoints).IsRequired(false);
            entity.Property(x => x.Orden).IsRequired();

            entity.HasOne(x => x.Plan)
                .WithMany(x => x.Levels)
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoyaltyReward>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(x => x.PuntosRequeridos).IsRequired();
            entity.Property(x => x.CantidadDisponible).IsRequired();
            entity.Property(x => x.Activo).IsRequired();

            entity.HasOne(x => x.Plan)
                .WithMany(x => x.Rewards)
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoyaltyAccount>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Points).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.UserId).IsUnique();

            entity.HasOne(x => x.Plan)
                .WithMany()
                .HasForeignKey(x => x.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoyaltyRedemption>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RewardName).IsRequired().HasMaxLength(200);
            entity.Property(x => x.PointsSpent).IsRequired();
            entity.Property(x => x.RedeemedAt).IsRequired();

            entity.HasOne(x => x.LoyaltyAccount)
                .WithMany(x => x.Redemptions)
                .HasForeignKey(x => x.LoyaltyAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Reward)
                .WithMany()
                .HasForeignKey(x => x.RewardId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

using BookingPlatform.Application.Authentication.Interfaces;
using BookingPlatform.Application.Authentication.DTOs;
using BookingPlatform.Application.Authentication.Commands.Login;
using BookingPlatform.Application.Authentication.Commands.Register;
using BookingPlatform.Application.Authentication.Commands.UpdateCurrentUser;
using BookingPlatform.Application.Authentication.Queries.GetCurrentUser;
using BookingPlatform.Application.Appointments.Commands.CancelAppointment;
using BookingPlatform.Application.Appointments.Commands.CompleteAppointment;
using BookingPlatform.Application.Appointments.Commands.CreateAppointment;
using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Application.Appointments.Queries.GetAppointmentById;
using BookingPlatform.Application.Appointments.Queries.GetAppointments;
using BookingPlatform.Application.Appointments.Queries.GetAvailableSlots;
using BookingPlatform.Application.Services.Commands.CreateService;
using BookingPlatform.Application.Services.Commands.DeleteService;
using BookingPlatform.Application.Services.Commands.UpdateService;
using BookingPlatform.Application.Services.DTOs;
using BookingPlatform.Application.Services.Queries.GetServiceById;
using BookingPlatform.Application.Services.Queries.GetServices;
using BookingPlatform.Application.Workers.Commands.CreateWorker;
using BookingPlatform.Application.Workers.DTOs;
using BookingPlatform.Application.Workers.Queries.GetWorkerById;
using BookingPlatform.Application.Workers.Queries.GetWorkers;
using BookingPlatform.Application.Admin.Queries.GetCustomers;
using BookingPlatform.Application.Common.DTOs;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Dashboard.DTOs;
using BookingPlatform.Application.Dashboard.Queries.GetDashboard;
using BookingPlatform.Application.Loyalty.Admin.Commands.CreateLoyaltyPlan;
using BookingPlatform.Application.Loyalty.Admin.Commands.DeleteLoyaltyPlan;
using BookingPlatform.Application.Loyalty.Admin.Commands.UpdateLoyaltyPlan;
using BookingPlatform.Application.Loyalty.Admin.DTOs;
using BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyPlanById;
using BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyPlans;
using BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyStats;
using BookingPlatform.Application.Loyalty.Commands.RedeemReward;
using BookingPlatform.Application.Loyalty.DTOs;
using BookingPlatform.Application.Loyalty.Queries.GetLoyalty;
using BookingPlatform.Infrastructure.Authentication;
using BookingPlatform.Infrastructure.Configurations;
using BookingPlatform.Infrastructure.Data;
using BookingPlatform.Infrastructure.Persistence.Repositories;
using BookingPlatform.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Infrastructure.ServiceExtensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PostgreSqlOptions>(
            configuration.GetSection("Infrastructure:PostgreSQL"));

        services.Configure<JwtOptions>(
            configuration.GetSection("Authentication:Jwt"));

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;

            options.UseNpgsql(dbOptions.ConnectionString);
        });

        services.AddValidatorsFromAssembly(typeof(RegisterCommandValidator).Assembly);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IWorkerRepository, WorkerRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<ILoyaltyRepository, LoyaltyRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRequestHandler<LoginCommand, AuthResponseDto>, LoginCommandHandler>();
        services.AddScoped<IRequestHandler<RegisterCommand, AuthResponseDto>, RegisterCommandHandler>();
        services.AddScoped<IRequestHandler<GetCurrentUserQuery, UserDto>, GetCurrentUserQueryHandler>();
        services.AddScoped<IRequestHandler<UpdateCurrentUserCommand, UserDto>, UpdateCurrentUserCommandHandler>();
        services.AddScoped<IRequestHandler<CreateServiceCommand, ServiceDto>, CreateServiceCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateServiceCommand, ServiceDto>, UpdateServiceCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteServiceCommand, Unit>, DeleteServiceCommandHandler>();
        services.AddScoped<IRequestHandler<GetServicesQuery, IReadOnlyList<ServiceDto>>, GetServicesQueryHandler>();
        services.AddScoped<IRequestHandler<GetServiceByIdQuery, ServiceDto>, GetServiceByIdQueryHandler>();
        services.AddScoped<IRequestHandler<CreateWorkerCommand, WorkerDto>, CreateWorkerCommandHandler>();
        services.AddScoped<IRequestHandler<GetWorkersQuery, IReadOnlyList<WorkerDto>>, GetWorkersQueryHandler>();
        services.AddScoped<IRequestHandler<GetWorkerByIdQuery, WorkerDto>, GetWorkerByIdQueryHandler>();
        services.AddScoped<IRequestHandler<GetCustomersQuery, IReadOnlyList<UserDto>>, GetCustomersQueryHandler>();
        services.AddScoped<IRequestHandler<CreateAppointmentCommand, AppointmentDto>, CreateAppointmentCommandHandler>();
        services.AddScoped<IRequestHandler<CancelAppointmentCommand, Unit>, CancelAppointmentCommandHandler>();
        services.AddScoped<IRequestHandler<CompleteAppointmentCommand, AppointmentDto>, CompleteAppointmentCommandHandler>();
        services.AddScoped<IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>>, GetAppointmentsQueryHandler>();
        services.AddScoped<IRequestHandler<GetAppointmentByIdQuery, AppointmentDto>, GetAppointmentByIdQueryHandler>();
        services.AddScoped<IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<string>>, GetAvailableSlotsQueryHandler>();
        services.AddScoped<IRequestHandler<GetDashboardQuery, DashboardResponseDto>, GetDashboardQueryHandler>();
        services.AddScoped<IRequestHandler<GetLoyaltyQuery, LoyaltyDto>, GetLoyaltyQueryHandler>();
        services.AddScoped<IRequestHandler<RedeemRewardCommand, LoyaltyDto>, RedeemRewardCommandHandler>();
        services.AddScoped<IRequestHandler<GetLoyaltyPlansQuery, IReadOnlyList<AdminLoyaltyPlanDto>>, GetLoyaltyPlansQueryHandler>();
        services.AddScoped<IRequestHandler<GetLoyaltyPlanByIdQuery, AdminLoyaltyPlanDto>, GetLoyaltyPlanByIdQueryHandler>();
        services.AddScoped<IRequestHandler<CreateLoyaltyPlanCommand, AdminLoyaltyPlanDto>, CreateLoyaltyPlanCommandHandler>();
        services.AddScoped<IRequestHandler<UpdateLoyaltyPlanCommand, AdminLoyaltyPlanDto>, UpdateLoyaltyPlanCommandHandler>();
        services.AddScoped<IRequestHandler<DeleteLoyaltyPlanCommand, Unit>, DeleteLoyaltyPlanCommandHandler>();
        services.AddScoped<IRequestHandler<GetLoyaltyStatsQuery, AdminLoyaltyStatsDto>, GetLoyaltyStatsQueryHandler>();

        return services;
    }
}
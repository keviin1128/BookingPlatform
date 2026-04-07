using BookingPlatform.Application.Authentication.Interfaces;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Infrastructure.Authentication;
using BookingPlatform.Infrastructure.Configurations;
using BookingPlatform.Infrastructure.Data;
using BookingPlatform.Infrastructure.Persistence.Repositories;
using BookingPlatform.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BookingPlatform.Infrastructure.ServiceExtensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PostgreSqlOptions>(
            configuration.GetSection("Infrastructure:PostgreSQL"));

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<PostgreSqlOptions>>().Value;

            options.UseNpgsql(dbOptions.ConnectionString);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
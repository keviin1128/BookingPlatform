namespace BookingPlatform.Infrastructure.Configurations;

public class InfrastructureOptions
{
    public PostgreSqlOptions PostgreSQL { get; set; } = new();
}

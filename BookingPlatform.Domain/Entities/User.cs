using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string? Nombre { get; set; }

    public string? Email { get; set; }

    public string Telefono { get; set; } = null!;

    public Role Role { get; set; } = Role.Customer;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

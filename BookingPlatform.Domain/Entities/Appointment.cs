using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }

    public string Servicio { get; set; } = string.Empty;

    public Guid ServicioId { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly Hora { get; set; }

    public AppointmentStatus Estado { get; set; } = AppointmentStatus.Pending;

    public decimal Precio { get; set; }

    public int Duracion { get; set; }

    public Guid TrabajadorId { get; set; }

    public string TrabajadorNombre { get; set; } = string.Empty;

    public Guid? ClienteId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public string? ClienteEmail { get; set; }

    public string? ClienteTelefono { get; set; }

    public bool ClienteRegistrado { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

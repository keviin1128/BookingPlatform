using BookingPlatform.Application.Appointments.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace BookingPlatform.Application.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand : IRequest<AppointmentDto>
{
    public string Servicio { get; init; } = string.Empty;

    public Guid ServicioId { get; init; }

    public DateOnly Fecha { get; init; }

    public string Hora { get; init; } = string.Empty;

    public decimal Precio { get; init; }

    public int Duracion { get; init; }

    public Guid TrabajadorId { get; init; }

    public string TrabajadorNombre { get; init; } = string.Empty;

    public string? ClienteNombre { get; init; }

    public string? ClienteEmail { get; init; }

    public string? ClienteTelefono { get; init; }

    public bool? ClienteRegistrado { get; init; }

    [JsonIgnore]
    public Guid? UsuarioAutenticadoId { get; init; }
}

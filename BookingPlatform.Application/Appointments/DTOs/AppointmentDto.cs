using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.Appointments.DTOs;

public record AppointmentDto(
    Guid Id,
    string Servicio,
    Guid ServicioId,
    string Fecha,
    string Hora,
    string Estado,
    decimal Precio,
    int Duracion,
    Guid TrabajadorId,
    string TrabajadorNombre,
    Guid? ClienteId,
    string ClienteNombre,
    string? ClienteEmail,
    string? ClienteTelefono,
    bool ClienteRegistrado)
{
    public static AppointmentDto FromDomain(Appointment appointment)
    {
        return new AppointmentDto(
            appointment.Id,
            appointment.Servicio,
            appointment.ServicioId,
            appointment.Fecha.ToString("yyyy-MM-dd"),
            appointment.Hora.ToString("HH:mm"),
            MapStatus(appointment.Estado),
            appointment.Precio,
            appointment.Duracion,
            appointment.TrabajadorId,
            appointment.TrabajadorNombre,
            appointment.ClienteId,
            appointment.ClienteNombre,
            appointment.ClienteEmail,
            appointment.ClienteTelefono,
            appointment.ClienteRegistrado);
    }

    private static string MapStatus(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Pending => "pendiente",
            AppointmentStatus.Confirmed => "confirmada",
            AppointmentStatus.Completed => "completada",
            AppointmentStatus.Cancelled => "cancelada",
            _ => "pendiente"
        };
    }
}

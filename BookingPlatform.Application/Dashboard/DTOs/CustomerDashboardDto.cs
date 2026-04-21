using BookingPlatform.Application.Appointments.DTOs;

namespace BookingPlatform.Application.Dashboard.DTOs;

public record CustomerDashboardDto(
    AppointmentDto? ProximaCita,
    IReadOnlyList<AppointmentDto> ProximasCitas,
    int CitasCompletadas,
    int PuntosLealtad,
    string NivelLealtad,
    int PuntosParaSiguienteNivel,
    string SiguienteNivel);
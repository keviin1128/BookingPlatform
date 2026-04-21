using BookingPlatform.Application.Appointments.DTOs;

namespace BookingPlatform.Application.Dashboard.DTOs;

public record AdminDashboardDto(
    int ClientesTotales,
    int TrabajadoresTotales,
    int CitasTotales,
    int CitasPendientes,
    int CitasConfirmadas,
    int CitasCompletadas,
    int CitasCanceladas,
    IReadOnlyList<AppointmentDto> AgendaGeneral,
    string EstadoOperacional);
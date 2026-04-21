using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Application.Workers.DTOs;

namespace BookingPlatform.Application.Dashboard.DTOs;

public record WorkerDashboardDto(
    WorkerDto Trabajador,
    IReadOnlyList<AppointmentDto> Pendientes,
    IReadOnlyList<AppointmentDto> Completadas,
    int TotalPendientes,
    int TotalCompletadas);
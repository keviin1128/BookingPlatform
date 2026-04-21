using BookingPlatform.Application.Common.DTOs;

namespace BookingPlatform.Application.Dashboard.DTOs;

public record DashboardResponseDto(
    UserDto Usuario,
    string Rol,
    CustomerDashboardDto? Cliente,
    WorkerDashboardDto? Trabajador,
    AdminDashboardDto? Admin);
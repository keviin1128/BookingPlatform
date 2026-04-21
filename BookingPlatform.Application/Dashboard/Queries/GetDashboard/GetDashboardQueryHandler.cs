using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Application.Common.DTOs;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Dashboard.DTOs;
using BookingPlatform.Application.Loyalty.DTOs;
using BookingPlatform.Application.Workers.DTOs;
using BookingPlatform.Domain.Enums;
using MediatR;

namespace BookingPlatform.Application.Dashboard.Queries.GetDashboard;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardResponseDto>
{
    private const int LoyaltyPointsPerLevel = 5;

    private readonly IUserRepository _userRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IWorkerRepository _workerRepository;
    private readonly ILoyaltyRepository? _loyaltyRepository;

    public GetDashboardQueryHandler(
        IUserRepository userRepository,
        IAppointmentRepository appointmentRepository,
        IWorkerRepository workerRepository,
        ILoyaltyRepository? loyaltyRepository = null)
    {
        _userRepository = userRepository;
        _appointmentRepository = appointmentRepository;
        _workerRepository = workerRepository;
        _loyaltyRepository = loyaltyRepository;
    }

    public async Task<DashboardResponseDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.RequesterUserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");
        var userDto = UserDto.FromDomain(user);

        return request.RequesterRole switch
        {
            Role.Admin => await BuildAdminDashboardAsync(user, userDto, cancellationToken),
            Role.Worker => await BuildWorkerDashboardAsync(user, userDto, cancellationToken),
            Role.Customer => await BuildCustomerDashboardAsync(user, userDto, cancellationToken),
            _ => throw new UnauthorizedAccessException("No tienes permisos para ver este dashboard.")
        };
    }

    private async Task<DashboardResponseDto> BuildCustomerDashboardAsync(
        BookingPlatform.Domain.Entities.User user,
        UserDto userDto,
        CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetByCustomerIdAsync(user.Id, cancellationToken);
        var completedAppointments = appointments
            .Where(x => x.Estado == AppointmentStatus.Completed)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Hora)
            .ToList();

        var upcomingAppointments = appointments
            .Where(x => x.Estado is AppointmentStatus.Pending or AppointmentStatus.Confirmed)
            .Where(IsUpcoming)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Hora)
            .Select(AppointmentDto.FromDomain)
            .ToList();

        var loyaltySnapshot = await GetLoyaltySnapshotAsync(user.Id, completedAppointments.Count, cancellationToken);
        var loyaltyPoints = loyaltySnapshot.Points;
        var levelIndex = loyaltySnapshot.LevelIndex;
        var pointsToNextLevel = loyaltySnapshot.PointsToNextLevel;

        return new DashboardResponseDto(
            userDto,
            userDto.Rol,
            new CustomerDashboardDto(
                upcomingAppointments.FirstOrDefault(),
                upcomingAppointments.Take(3).ToList(),
                completedAppointments.Count,
                loyaltyPoints,
                $"Nivel {levelIndex}",
                pointsToNextLevel,
                $"Nivel {levelIndex + 1}"),
            null,
            null);
    }

    private async Task<DashboardResponseDto> BuildWorkerDashboardAsync(
        BookingPlatform.Domain.Entities.User user,
        UserDto userDto,
        CancellationToken cancellationToken)
    {
        var worker = await _workerRepository.GetByUserIdAsync(user.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Trabajador no encontrado.");

        var appointments = await _appointmentRepository.GetByWorkerIdAsync(worker.Id, cancellationToken);

        var pendingAppointments = appointments
            .Where(x => x.Estado is AppointmentStatus.Pending or AppointmentStatus.Confirmed)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Hora)
            .Select(AppointmentDto.FromDomain)
            .ToList();

        var completedAppointments = appointments
            .Where(x => x.Estado == AppointmentStatus.Completed)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Hora)
            .Select(AppointmentDto.FromDomain)
            .ToList();

        return new DashboardResponseDto(
            userDto,
            userDto.Rol,
            null,
            new WorkerDashboardDto(
                WorkerDto.FromDomain(worker, includeAgenda: true),
                pendingAppointments,
                completedAppointments,
                pendingAppointments.Count,
                completedAppointments.Count),
            null);
    }

    private async Task<DashboardResponseDto> BuildAdminDashboardAsync(
        BookingPlatform.Domain.Entities.User user,
        UserDto userDto,
        CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        var workers = await _workerRepository.GetAllAsync(null, null, cancellationToken);

        var orderedAppointments = appointments
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Hora)
            .Select(AppointmentDto.FromDomain)
            .ToList();

        var completedCount = appointments.Count(x => x.Estado == AppointmentStatus.Completed);
        var pendingCount = appointments.Count(x => x.Estado == AppointmentStatus.Pending);
        var confirmedCount = appointments.Count(x => x.Estado == AppointmentStatus.Confirmed);
        var cancelledCount = appointments.Count(x => x.Estado == AppointmentStatus.Cancelled);
        var customerCount = appointments
            .Where(x => x.ClienteId.HasValue)
            .Select(x => x.ClienteId!.Value)
            .Distinct()
            .Count();

        var activeAppointments = pendingCount + confirmedCount;
        var estadoOperacional = appointments.Count == 0
            ? "sin actividad"
            : activeAppointments == 0
                ? "estable"
                : activeAppointments <= completedCount
                    ? "operativo"
                    : activeAppointments <= completedCount * 2
                        ? "carga media"
                        : "alta demanda";

        return new DashboardResponseDto(
            userDto,
            userDto.Rol,
            null,
            null,
            new AdminDashboardDto(
                customerCount,
                workers.Count,
                appointments.Count,
                pendingCount,
                confirmedCount,
                completedCount,
                cancelledCount,
                orderedAppointments,
                estadoOperacional));
    }

    private static bool IsUpcoming(BookingPlatform.Domain.Entities.Appointment appointment)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = TimeOnly.FromDateTime(DateTime.UtcNow);

        return appointment.Fecha > today || (appointment.Fecha == today && appointment.Hora >= now);
    }

    private async Task<LoyaltySnapshot> GetLoyaltySnapshotAsync(Guid userId, int fallbackPoints, CancellationToken cancellationToken)
    {
        if (_loyaltyRepository is null)
        {
            return BuildFallbackSnapshot(fallbackPoints);
        }

        var account = await _loyaltyRepository.GetOrCreateAccountAsync(userId, cancellationToken);
        var plan = await _loyaltyRepository.GetOrCreateActivePlanAsync(cancellationToken);
        var levels = plan.Levels.OrderBy(x => x.Orden).ToList();

        if (levels.Count == 0)
        {
            return BuildFallbackSnapshot(fallbackPoints);
        }

        var currentLevel = levels.LastOrDefault(level => account.Points >= level.MinPoints) ?? levels.First();
        var nextLevel = levels.FirstOrDefault(level => level.MinPoints > account.Points);

        return new LoyaltySnapshot(
            account.Points,
            ExtractLevelIndex(currentLevel.Nombre),
            nextLevel is null ? 0 : Math.Max(0, nextLevel.MinPoints - account.Points));
    }

    private static LoyaltySnapshot BuildFallbackSnapshot(int fallbackPoints)
    {
        var levelIndex = fallbackPoints / LoyaltyPointsPerLevel + 1;
        var pointsToNextLevel = LoyaltyPointsPerLevel - (fallbackPoints % LoyaltyPointsPerLevel);

        return new LoyaltySnapshot(fallbackPoints, levelIndex, pointsToNextLevel);
    }

    private static int ExtractLevelIndex(string levelName)
    {
        var digits = new string(levelName.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var levelIndex) ? levelIndex : 1;
    }

    private sealed record LoyaltySnapshot(int Points, int LevelIndex, int PointsToNextLevel);
}
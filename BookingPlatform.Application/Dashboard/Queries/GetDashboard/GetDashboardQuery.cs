using BookingPlatform.Application.Dashboard.DTOs;
using BookingPlatform.Domain.Enums;
using MediatR;

namespace BookingPlatform.Application.Dashboard.Queries.GetDashboard;

public record GetDashboardQuery(Role RequesterRole, Guid RequesterUserId) : IRequest<DashboardResponseDto>;
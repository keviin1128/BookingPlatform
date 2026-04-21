using BookingPlatform.Application.Dashboard.DTOs;
using BookingPlatform.Application.Dashboard.Queries.GetDashboard;
using BookingPlatform.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingPlatform.API.Controllers;

[Route("dashboard")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IRequestHandler<GetDashboardQuery, DashboardResponseDto> _getDashboardHandler;

    public DashboardController(IRequestHandler<GetDashboardQuery, DashboardResponseDto> getDashboardHandler)
    {
        _getDashboardHandler = getDashboardHandler;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponseDto>> Get()
    {
        var role = GetCurrentRole();
        var userId = GetCurrentUserId();

        if (role is null || userId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _getDashboardHandler.Handle(
            new GetDashboardQuery(role.Value, userId.Value),
            HttpContext.RequestAborted);

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private Role? GetCurrentRole()
    {
        var value = User.FindFirstValue(ClaimTypes.Role);

        return Enum.TryParse<Role>(value, ignoreCase: true, out var role) ? role : null;
    }
}
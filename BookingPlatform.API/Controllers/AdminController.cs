using BookingPlatform.Application.Workers.Commands.CreateWorker;
using BookingPlatform.Application.Admin.Queries.GetCustomers;
using BookingPlatform.Application.Dashboard.DTOs;
using BookingPlatform.Application.Dashboard.Queries.GetDashboard;
using BookingPlatform.Application.Loyalty.Admin.Commands.CreateLoyaltyPlan;
using BookingPlatform.Application.Loyalty.Admin.Commands.DeleteLoyaltyPlan;
using BookingPlatform.Application.Loyalty.Admin.Commands.UpdateLoyaltyPlan;
using BookingPlatform.Application.Loyalty.Admin.DTOs;
using BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyPlanById;
using BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyPlans;
using BookingPlatform.Application.Loyalty.Admin.Queries.GetLoyaltyStats;
using BookingPlatform.Application.Workers.DTOs;
using BookingPlatform.Application.Workers.Queries.GetWorkers;
using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Application.Appointments.Queries.GetAppointments;
using BookingPlatform.Application.Common.DTOs;
using BookingPlatform.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingPlatform.API.Controllers;

[Route("admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IRequestHandler<GetCustomersQuery, IReadOnlyList<UserDto>> _getCustomersHandler;
    private readonly IRequestHandler<GetWorkersQuery, IReadOnlyList<WorkerDto>> _getWorkersHandler;
    private readonly IRequestHandler<CreateWorkerCommand, WorkerDto> _createWorkerHandler;
    private readonly IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>> _getAppointmentsHandler;
    private readonly IRequestHandler<GetDashboardQuery, DashboardResponseDto> _getDashboardHandler;
    private readonly IRequestHandler<GetLoyaltyPlansQuery, IReadOnlyList<AdminLoyaltyPlanDto>> _getLoyaltyPlansHandler;
    private readonly IRequestHandler<GetLoyaltyPlanByIdQuery, AdminLoyaltyPlanDto> _getLoyaltyPlanByIdHandler;
    private readonly IRequestHandler<CreateLoyaltyPlanCommand, AdminLoyaltyPlanDto> _createLoyaltyPlanHandler;
    private readonly IRequestHandler<UpdateLoyaltyPlanCommand, AdminLoyaltyPlanDto> _updateLoyaltyPlanHandler;
    private readonly IRequestHandler<DeleteLoyaltyPlanCommand, Unit> _deleteLoyaltyPlanHandler;
    private readonly IRequestHandler<GetLoyaltyStatsQuery, AdminLoyaltyStatsDto> _getLoyaltyStatsHandler;

    public AdminController(
        IRequestHandler<GetCustomersQuery, IReadOnlyList<UserDto>> getCustomersHandler,
        IRequestHandler<GetWorkersQuery, IReadOnlyList<WorkerDto>> getWorkersHandler,
        IRequestHandler<CreateWorkerCommand, WorkerDto> createWorkerHandler,
        IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>> getAppointmentsHandler,
        IRequestHandler<GetDashboardQuery, DashboardResponseDto> getDashboardHandler,
        IRequestHandler<GetLoyaltyPlansQuery, IReadOnlyList<AdminLoyaltyPlanDto>> getLoyaltyPlansHandler,
        IRequestHandler<GetLoyaltyPlanByIdQuery, AdminLoyaltyPlanDto> getLoyaltyPlanByIdHandler,
        IRequestHandler<CreateLoyaltyPlanCommand, AdminLoyaltyPlanDto> createLoyaltyPlanHandler,
        IRequestHandler<UpdateLoyaltyPlanCommand, AdminLoyaltyPlanDto> updateLoyaltyPlanHandler,
        IRequestHandler<DeleteLoyaltyPlanCommand, Unit> deleteLoyaltyPlanHandler,
        IRequestHandler<GetLoyaltyStatsQuery, AdminLoyaltyStatsDto> getLoyaltyStatsHandler)
    {
        _getCustomersHandler = getCustomersHandler;
        _getWorkersHandler = getWorkersHandler;
        _createWorkerHandler = createWorkerHandler;
        _getAppointmentsHandler = getAppointmentsHandler;
        _getDashboardHandler = getDashboardHandler;
        _getLoyaltyPlansHandler = getLoyaltyPlansHandler;
        _getLoyaltyPlanByIdHandler = getLoyaltyPlanByIdHandler;
        _createLoyaltyPlanHandler = createLoyaltyPlanHandler;
        _updateLoyaltyPlanHandler = updateLoyaltyPlanHandler;
        _deleteLoyaltyPlanHandler = deleteLoyaltyPlanHandler;
        _getLoyaltyStatsHandler = getLoyaltyStatsHandler;
    }

    [HttpGet("customers")]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetCustomers([FromQuery] string? search)
    {
        var result = await _getCustomersHandler.Handle(new GetCustomersQuery(search), HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpGet("workers")]
    public async Task<ActionResult<IReadOnlyList<WorkerDto>>> GetWorkers([FromQuery] string? search)
    {
        var result = await _getWorkersHandler.Handle(new GetWorkersQuery(search, null), HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpGet("appointments")]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetAppointments()
    {
        var result = await _getAppointmentsHandler.Handle(
            new GetAppointmentsQuery(Role.Admin, Guid.Empty),
            HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminDashboardDto>> GetStats()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _getDashboardHandler.Handle(
            new GetDashboardQuery(Role.Admin, userId.Value),
            HttpContext.RequestAborted);

        return Ok(result.Admin);
    }

    [HttpPost("workers")]
    public async Task<ActionResult<WorkerDto>> CreateWorker([FromBody] CreateWorkerCommand command)
    {
        var result = await _createWorkerHandler.Handle(command, HttpContext.RequestAborted);

        return CreatedAtAction(nameof(WorkersController.GetById), "Workers", new { id = result.Id }, result);
    }

    [HttpGet("loyalty/plans")]
    public async Task<ActionResult<IReadOnlyList<AdminLoyaltyPlanDto>>> GetLoyaltyPlans()
    {
        var result = await _getLoyaltyPlansHandler.Handle(new GetLoyaltyPlansQuery(), HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpGet("loyalty/plans/{planId:guid}")]
    public async Task<ActionResult<AdminLoyaltyPlanDto>> GetLoyaltyPlanById([FromRoute] Guid planId)
    {
        var result = await _getLoyaltyPlanByIdHandler.Handle(new GetLoyaltyPlanByIdQuery(planId), HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpPost("loyalty/plans")]
    public async Task<ActionResult<AdminLoyaltyPlanDto>> CreateLoyaltyPlan([FromBody] CreateLoyaltyPlanCommand command)
    {
        var result = await _createLoyaltyPlanHandler.Handle(command, HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetLoyaltyPlanById), new { planId = result.Id }, result);
    }

    [HttpPut("loyalty/plans/{planId:guid}")]
    public async Task<ActionResult<AdminLoyaltyPlanDto>> UpdateLoyaltyPlan([FromRoute] Guid planId, [FromBody] UpdateLoyaltyPlanCommand command)
    {
        var result = await _updateLoyaltyPlanHandler.Handle(command with { PlanId = planId }, HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpDelete("loyalty/plans/{planId:guid}")]
    public async Task<IActionResult> DeleteLoyaltyPlan([FromRoute] Guid planId)
    {
        await _deleteLoyaltyPlanHandler.Handle(new DeleteLoyaltyPlanCommand { PlanId = planId }, HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpGet("loyalty/stats")]
    public async Task<ActionResult<AdminLoyaltyStatsDto>> GetLoyaltyStats()
    {
        var result = await _getLoyaltyStatsHandler.Handle(new GetLoyaltyStatsQuery(), HttpContext.RequestAborted);

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}

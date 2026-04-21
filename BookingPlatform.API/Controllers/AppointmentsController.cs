using BookingPlatform.Application.Appointments.Commands.CancelAppointment;
using BookingPlatform.Application.Appointments.Commands.CompleteAppointment;
using BookingPlatform.Application.Appointments.Commands.CreateAppointment;
using BookingPlatform.Application.Appointments.DTOs;
using BookingPlatform.Application.Appointments.Queries.GetAppointmentById;
using BookingPlatform.Application.Appointments.Queries.GetAppointments;
using BookingPlatform.Application.Appointments.Queries.GetAvailableSlots;
using BookingPlatform.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingPlatform.API.Controllers;

[Route("appointments")]
[ApiController]
public class AppointmentsController : ControllerBase
{
    private readonly IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>> _getAppointmentsHandler;
    private readonly IRequestHandler<GetAppointmentByIdQuery, AppointmentDto> _getAppointmentByIdHandler;
    private readonly IRequestHandler<CreateAppointmentCommand, AppointmentDto> _createAppointmentHandler;
    private readonly IRequestHandler<CancelAppointmentCommand, Unit> _cancelAppointmentHandler;
    private readonly IRequestHandler<CompleteAppointmentCommand, AppointmentDto> _completeAppointmentHandler;
    private readonly IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<string>> _getAvailableSlotsHandler;

    public AppointmentsController(
        IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>> getAppointmentsHandler,
        IRequestHandler<GetAppointmentByIdQuery, AppointmentDto> getAppointmentByIdHandler,
        IRequestHandler<CreateAppointmentCommand, AppointmentDto> createAppointmentHandler,
        IRequestHandler<CancelAppointmentCommand, Unit> cancelAppointmentHandler,
        IRequestHandler<CompleteAppointmentCommand, AppointmentDto> completeAppointmentHandler,
        IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<string>> getAvailableSlotsHandler)
    {
        _getAppointmentsHandler = getAppointmentsHandler;
        _getAppointmentByIdHandler = getAppointmentByIdHandler;
        _createAppointmentHandler = createAppointmentHandler;
        _cancelAppointmentHandler = cancelAppointmentHandler;
        _completeAppointmentHandler = completeAppointmentHandler;
        _getAvailableSlotsHandler = getAvailableSlotsHandler;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetAll()
    {
        var role = GetCurrentRole();
        var userId = GetCurrentUserId();

        if (role is null || userId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _getAppointmentsHandler.Handle(
            new GetAppointmentsQuery(role.Value, userId.Value),
            HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<AppointmentDto>> GetById([FromRoute] Guid id)
    {
        var role = GetCurrentRole();
        var userId = GetCurrentUserId();

        if (role is null || userId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _getAppointmentByIdHandler.Handle(
            new GetAppointmentByIdQuery(id, role.Value, userId.Value),
            HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentCommand command)
    {
        var userId = GetCurrentUserId();
        var result = await _createAppointmentHandler.Handle(
            command with { UsuarioAutenticadoId = userId },
            HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Cancel([FromRoute] Guid id)
    {
        var role = GetCurrentRole();
        var userId = GetCurrentUserId();

        if (role is null || userId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        await _cancelAppointmentHandler.Handle(
            new CancelAppointmentCommand
            {
                Id = id,
                RequesterRole = role.Value,
                RequesterUserId = userId.Value
            },
            HttpContext.RequestAborted);

        return NoContent();
    }

    [HttpPut("{id:guid}/complete")]
    [Authorize(Roles = "Admin,Worker")]
    public async Task<ActionResult<AppointmentDto>> Complete([FromRoute] Guid id)
    {
        var role = GetCurrentRole();
        var userId = GetCurrentUserId();

        if (role is null || userId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _completeAppointmentHandler.Handle(
            new CompleteAppointmentCommand
            {
                Id = id,
                RequesterRole = role.Value,
                RequesterUserId = userId.Value
            },
            HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpGet("slots")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<string>>> GetSlots(
        [FromQuery] DateOnly date,
        [FromQuery] Guid workerId,
        [FromQuery] Guid serviceId,
        [FromQuery] int? duration)
    {
        var result = await _getAvailableSlotsHandler.Handle(
            new GetAvailableSlotsQuery(date, workerId, serviceId, duration),
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

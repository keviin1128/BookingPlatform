using BookingPlatform.Application.Services.Commands.CreateService;
using BookingPlatform.Application.Services.Commands.DeleteService;
using BookingPlatform.Application.Services.Commands.UpdateService;
using BookingPlatform.Application.Services.DTOs;
using BookingPlatform.Application.Services.Queries.GetServiceById;
using BookingPlatform.Application.Services.Queries.GetServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.API.Controllers;

[Route("services")]
[ApiController]
public class ServicesController : ControllerBase
{
    private readonly IRequestHandler<GetServicesQuery, IReadOnlyList<ServiceDto>> _getServicesHandler;
    private readonly IRequestHandler<GetServiceByIdQuery, ServiceDto> _getServiceByIdHandler;
    private readonly IRequestHandler<CreateServiceCommand, ServiceDto> _createServiceHandler;
    private readonly IRequestHandler<UpdateServiceCommand, ServiceDto> _updateServiceHandler;
    private readonly IRequestHandler<DeleteServiceCommand, Unit> _deleteServiceHandler;

    public ServicesController(
        IRequestHandler<GetServicesQuery, IReadOnlyList<ServiceDto>> getServicesHandler,
        IRequestHandler<GetServiceByIdQuery, ServiceDto> getServiceByIdHandler,
        IRequestHandler<CreateServiceCommand, ServiceDto> createServiceHandler,
        IRequestHandler<UpdateServiceCommand, ServiceDto> updateServiceHandler,
        IRequestHandler<DeleteServiceCommand, Unit> deleteServiceHandler)
    {
        _getServicesHandler = getServicesHandler;
        _getServiceByIdHandler = getServiceByIdHandler;
        _createServiceHandler = createServiceHandler;
        _updateServiceHandler = updateServiceHandler;
        _deleteServiceHandler = deleteServiceHandler;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> GetAll()
    {
        var result = await _getServicesHandler.Handle(new GetServicesQuery(), HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ServiceDto>> GetById([FromRoute] Guid id)
    {
        var result = await _getServiceByIdHandler.Handle(new GetServiceByIdQuery(id), HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceDto>> Create([FromBody] CreateServiceCommand command)
    {
        var result = await _createServiceHandler.Handle(command, HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceDto>> Update([FromRoute] Guid id, [FromBody] UpdateServiceCommand command)
    {
        var result = await _updateServiceHandler.Handle(command with { Id = id }, HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _deleteServiceHandler.Handle(new DeleteServiceCommand { Id = id }, HttpContext.RequestAborted);

        return NoContent();
    }
}

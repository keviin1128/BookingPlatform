using BookingPlatform.Application.Workers.DTOs;
using BookingPlatform.Application.Workers.Queries.GetWorkerById;
using BookingPlatform.Application.Workers.Queries.GetWorkers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.API.Controllers;

[Route("workers")]
[ApiController]
public class WorkersController : ControllerBase
{
    private readonly IRequestHandler<GetWorkersQuery, IReadOnlyList<WorkerDto>> _getWorkersHandler;
    private readonly IRequestHandler<GetWorkerByIdQuery, WorkerDto> _getWorkerByIdHandler;

    public WorkersController(
        IRequestHandler<GetWorkersQuery, IReadOnlyList<WorkerDto>> getWorkersHandler,
        IRequestHandler<GetWorkerByIdQuery, WorkerDto> getWorkerByIdHandler)
    {
        _getWorkersHandler = getWorkersHandler;
        _getWorkerByIdHandler = getWorkerByIdHandler;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<WorkerDto>>> GetAll([FromQuery] Guid? serviceId)
    {
        var result = await _getWorkersHandler.Handle(new GetWorkersQuery(null, serviceId), HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<WorkerDto>> GetById([FromRoute] Guid id)
    {
        var result = await _getWorkerByIdHandler.Handle(new GetWorkerByIdQuery(id), HttpContext.RequestAborted);

        return Ok(result);
    }
}

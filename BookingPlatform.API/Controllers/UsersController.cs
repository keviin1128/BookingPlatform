using BookingPlatform.Application.Authentication.Commands.UpdateCurrentUser;
using BookingPlatform.Application.Authentication.Queries.GetCurrentUser;
using BookingPlatform.Application.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingPlatform.API.Controllers;

[Route("users")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IRequestHandler<GetCurrentUserQuery, UserDto> _getCurrentUserHandler;
    private readonly IRequestHandler<UpdateCurrentUserCommand, UserDto> _updateCurrentUserHandler;

    public UsersController(
        IRequestHandler<GetCurrentUserQuery, UserDto> getCurrentUserHandler,
        IRequestHandler<UpdateCurrentUserCommand, UserDto> updateCurrentUserHandler)
    {
        _getCurrentUserHandler = getCurrentUserHandler;
        _updateCurrentUserHandler = updateCurrentUserHandler;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _getCurrentUserHandler.Handle(new GetCurrentUserQuery(userId.Value), HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateCurrentUserCommand command)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _updateCurrentUserHandler.Handle(command with { UserId = userId.Value }, HttpContext.RequestAborted);

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
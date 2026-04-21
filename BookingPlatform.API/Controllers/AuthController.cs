using BookingPlatform.Application.Authentication.Commands.Login;
using BookingPlatform.Application.Authentication.Commands.Register;
using BookingPlatform.Application.Authentication.DTOs;
using BookingPlatform.Application.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.API.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IRequestHandler<LoginCommand, AuthResponseDto> _loginHandler;
    private readonly IRequestHandler<RegisterCommand, AuthResponseDto> _registerHandler;

    public AuthController(
        IRequestHandler<LoginCommand, AuthResponseDto> loginHandler,
        IRequestHandler<RegisterCommand, AuthResponseDto> registerHandler)
    {
        _loginHandler = loginHandler;
        _registerHandler = registerHandler;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterCommand command)
    {
        var result = await _registerHandler.Handle(command, HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
    {
        var result = await _loginHandler.Handle(command, HttpContext.RequestAborted);

        return Ok(result);
    }
}

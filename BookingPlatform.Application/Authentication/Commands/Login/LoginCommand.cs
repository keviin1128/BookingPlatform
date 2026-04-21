using BookingPlatform.Application.Authentication.DTOs;
using MediatR;

namespace BookingPlatform.Application.Authentication.Commands.Login;

public record LoginCommand(
    string Telefono
) : IRequest<AuthResponseDto>; 

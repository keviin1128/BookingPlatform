using BookingPlatform.Application.Authentication.DTOs;
using MediatR;

namespace BookingPlatform.Application.Authentication.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password
) : IRequest<AuthResponseDto>;

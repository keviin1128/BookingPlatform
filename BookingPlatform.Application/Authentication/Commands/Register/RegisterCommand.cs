using BookingPlatform.Application.Authentication.DTOs;
using MediatR;

namespace BookingPlatform.Application.Authentication.Commands.Register;

public record RegisterCommand(
    string Telefono,
    string? Nombre,
    string? Email
) : IRequest<AuthResponseDto>;

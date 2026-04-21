using BookingPlatform.Application.Common.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace BookingPlatform.Application.Authentication.Commands.UpdateCurrentUser;

public record UpdateCurrentUserCommand : IRequest<UserDto>
{
    [JsonIgnore]
    public Guid UserId { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Telefono { get; init; } = string.Empty;
}
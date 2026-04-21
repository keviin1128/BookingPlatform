using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.Common.DTOs;

public record UserDto(
    Guid Id,
    string Nombre,
    string Email,
    string Telefono,
    string Rol)
{
    public static UserDto FromDomain(User user)
    {
        return new UserDto(
            user.Id,
            user.Nombre ?? string.Empty,
            user.Email ?? string.Empty,
            user.Telefono,
            MapRole(user.Role));
    }

    private static string MapRole(Role role)
    {
        return role switch
        {
            Role.Admin => "admin",
            Role.Worker => "trabajador",
            Role.Customer => "cliente",
            _ => "cliente"
        };
    }
}
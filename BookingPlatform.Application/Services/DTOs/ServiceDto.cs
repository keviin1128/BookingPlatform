using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Services.DTOs;

public record ServiceDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    int Duracion,
    decimal Precio,
    bool Activo)
{
    public static ServiceDto FromDomain(Service service)
    {
        return new ServiceDto(
            service.Id,
            service.Nombre,
            service.Descripcion,
            service.Duracion,
            service.Precio,
            service.Activo);
    }
}

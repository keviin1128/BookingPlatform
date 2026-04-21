using BookingPlatform.Application.Services.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace BookingPlatform.Application.Services.Commands.UpdateService;

public record UpdateServiceCommand : IRequest<ServiceDto>
{
    [JsonIgnore]
    public Guid Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public int Duracion { get; init; }

    public decimal Precio { get; init; }

    public bool Activo { get; init; } = true;
}

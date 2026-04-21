using BookingPlatform.Application.Services.DTOs;
using MediatR;

namespace BookingPlatform.Application.Services.Commands.CreateService;

public record CreateServiceCommand(
    string Nombre,
    string Descripcion,
    int Duracion,
    decimal Precio,
    bool Activo = true) : IRequest<ServiceDto>;

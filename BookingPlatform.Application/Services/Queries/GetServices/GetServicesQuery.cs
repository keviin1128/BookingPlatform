using BookingPlatform.Application.Services.DTOs;
using MediatR;

namespace BookingPlatform.Application.Services.Queries.GetServices;

public record GetServicesQuery() : IRequest<IReadOnlyList<ServiceDto>>;

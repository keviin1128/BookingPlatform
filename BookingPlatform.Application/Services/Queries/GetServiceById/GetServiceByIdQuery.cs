using BookingPlatform.Application.Services.DTOs;
using MediatR;

namespace BookingPlatform.Application.Services.Queries.GetServiceById;

public record GetServiceByIdQuery(Guid Id) : IRequest<ServiceDto>;

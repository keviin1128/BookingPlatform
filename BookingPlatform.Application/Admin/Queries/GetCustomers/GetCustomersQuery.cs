using BookingPlatform.Application.Common.DTOs;
using MediatR;

namespace BookingPlatform.Application.Admin.Queries.GetCustomers;

public record GetCustomersQuery(string? Search) : IRequest<IReadOnlyList<UserDto>>;

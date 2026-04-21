using BookingPlatform.Application.Common.DTOs;
using MediatR;

namespace BookingPlatform.Application.Authentication.Queries.GetCurrentUser;

public record GetCurrentUserQuery(Guid UserId) : IRequest<UserDto>;
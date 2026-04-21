using BookingPlatform.Application.Common.DTOs;
using BookingPlatform.Application.Common.Interfaces;
using MediatR;

namespace BookingPlatform.Application.Authentication.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        return UserDto.FromDomain(user);
    }
}
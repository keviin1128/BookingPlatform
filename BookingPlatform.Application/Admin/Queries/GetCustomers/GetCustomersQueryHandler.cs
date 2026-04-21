using BookingPlatform.Application.Common.DTOs;
using BookingPlatform.Application.Common.Interfaces;
using MediatR;

namespace BookingPlatform.Application.Admin.Queries.GetCustomers;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetCustomersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _userRepository.GetCustomersAsync(request.Search, cancellationToken);

        return customers
            .OrderBy(x => x.Nombre)
            .Select(UserDto.FromDomain)
            .ToList();
    }
}

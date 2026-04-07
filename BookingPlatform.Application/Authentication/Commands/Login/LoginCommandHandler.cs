using BookingPlatform.Application.Authentication.DTOs;
using BookingPlatform.Application.Authentication.Interfaces;
using BookingPlatform.Application.Common.Interfaces;
using MediatR;

namespace BookingPlatform.Application.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email);

        if (user is null)
            throw new Exception("Credenciales incorrectas");

        var isPasswordValid = _passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!isPasswordValid)
            throw new Exception("Credenciales incorrectas");

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            token
        );
    }
}

using BookingPlatform.Application.Authentication.DTOs;
using BookingPlatform.Application.Authentication.Interfaces;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Common.Utilities;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IValidator<LoginCommand> _validator;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IValidator<LoginCommand> validator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _validator = validator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var normalizedPhone = PhoneNumberNormalizer.Normalize(command.Telefono);
        var user = await _userRepository.GetByPhoneAsync(normalizedPhone, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Credenciales incorrectas.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto(token, BookingPlatform.Application.Common.DTOs.UserDto.FromDomain(user));
    }
}

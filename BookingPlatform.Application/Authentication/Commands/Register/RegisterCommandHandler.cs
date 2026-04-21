using BookingPlatform.Application.Authentication.DTOs;
using BookingPlatform.Application.Authentication.Interfaces;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Common.Utilities;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Authentication.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtGenerator;
    private readonly IValidator<RegisterCommand> _validator;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtGenerator,
        IValidator<RegisterCommand> validator)
    {
        _userRepository = userRepository;
        _jwtGenerator = jwtGenerator;
        _validator = validator;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var normalizedPhone = PhoneNumberNormalizer.Normalize(request.Telefono);
        var existingUser = await _userRepository.GetByPhoneAsync(normalizedPhone, cancellationToken);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("El telefono ya esta registrado.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nombre = string.IsNullOrWhiteSpace(request.Nombre) ? string.Empty : request.Nombre.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? string.Empty : request.Email.Trim(),
            Telefono = normalizedPhone,
            Role = Role.Customer
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var token = _jwtGenerator.GenerateToken(user);

        return new AuthResponseDto(token, BookingPlatform.Application.Common.DTOs.UserDto.FromDomain(user));
    }
}

using BookingPlatform.Application.Common.DTOs;
using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Common.Utilities;
using BookingPlatform.Domain.Entities;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Authentication.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandHandler : IRequestHandler<UpdateCurrentUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<UpdateCurrentUserCommand> _validator;

    public UpdateCurrentUserCommandHandler(
        IUserRepository userRepository,
        IValidator<UpdateCurrentUserCommand> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<UserDto> Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("Usuario no encontrado.");
        }

        var normalizedPhone = PhoneNumberNormalizer.Normalize(request.Telefono);

        var existingUserWithPhone = await _userRepository.GetByPhoneAsync(normalizedPhone, cancellationToken);
        if (existingUserWithPhone is not null && existingUserWithPhone.Id != user.Id)
        {
            throw new InvalidOperationException("El telefono ya esta registrado.");
        }

        user.Nombre = string.IsNullOrWhiteSpace(request.Nombre) ? string.Empty : request.Nombre.Trim();
        user.Email = string.IsNullOrWhiteSpace(request.Email) ? string.Empty : request.Email.Trim();
        user.Telefono = normalizedPhone;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return UserDto.FromDomain(user);
    }
}
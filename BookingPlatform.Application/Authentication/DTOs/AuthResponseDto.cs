using BookingPlatform.Application.Common.DTOs;

namespace BookingPlatform.Application.Authentication.DTOs;

public record AuthResponseDto(
    string Token,
    UserDto User
);

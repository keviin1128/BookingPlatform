namespace BookingPlatform.Application.Authentication.DTOs;

public record AuthResponseDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Token
);

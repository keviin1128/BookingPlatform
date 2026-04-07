using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Authentication.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}

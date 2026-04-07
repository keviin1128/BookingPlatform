using BookingPlatform.Application.Authentication.Interfaces;
using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    public string GenerateToken(User user)
    {
        throw new NotImplementedException();
    }
}

using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Common.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetCustomersAsync(string? search, CancellationToken cancellationToken = default);
}

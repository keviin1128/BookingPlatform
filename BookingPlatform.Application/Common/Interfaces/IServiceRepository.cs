using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Common.Interfaces;

public interface IServiceRepository
{
    Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Service service, CancellationToken cancellationToken = default);

    Task UpdateAsync(Service service, CancellationToken cancellationToken = default);

    Task DeleteAsync(Service service, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

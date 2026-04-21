using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Common.Interfaces;

public interface IWorkerRepository
{
    Task AddAsync(Worker worker, CancellationToken cancellationToken = default);

    Task<Worker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Worker?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Worker>> GetAllAsync(string? search, Guid? serviceId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

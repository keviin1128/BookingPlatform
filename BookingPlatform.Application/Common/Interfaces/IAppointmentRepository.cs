using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Common.Interfaces;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetByWorkerAndDateAsync(Guid workerId, DateOnly date, CancellationToken cancellationToken = default);
}

using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatform.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _dbContext;

    public AppointmentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Appointments.AddAsync(appointment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        _dbContext.Appointments.Update(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Appointments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.TrabajadorId == workerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.ClienteId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByWorkerAndDateAsync(Guid workerId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .Where(x => x.TrabajadorId == workerId && x.Fecha == date)
            .ToListAsync(cancellationToken);
    }
}

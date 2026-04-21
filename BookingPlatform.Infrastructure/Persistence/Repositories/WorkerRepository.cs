using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatform.Infrastructure.Persistence.Repositories;

public class WorkerRepository : IWorkerRepository
{
    private readonly AppDbContext _dbContext;

    public WorkerRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Worker worker, CancellationToken cancellationToken = default)
    {
        await _dbContext.Workers.AddAsync(worker, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Worker?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workers
            .Include(x => x.User)
            .Include(x => x.WorkerServices)
            .Include(x => x.Agenda)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Worker?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workers
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.WorkerServices)
            .Include(x => x.Agenda)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Worker>> GetAllAsync(string? search, Guid? serviceId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Workers
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.WorkerServices)
            .Include(x => x.Agenda)
            .AsQueryable();

        if (serviceId.HasValue)
        {
            query = query.Where(x => x.WorkerServices.Any(ws => ws.ServiceId == serviceId.Value));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                (x.User.Nombre ?? string.Empty).ToLower().Contains(normalizedSearch) ||
                x.User.Telefono.Contains(normalizedSearch) ||
                x.Especialidad.ToLower().Contains(normalizedSearch));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workers.AnyAsync(x => x.UserId == userId, cancellationToken);
    }
}

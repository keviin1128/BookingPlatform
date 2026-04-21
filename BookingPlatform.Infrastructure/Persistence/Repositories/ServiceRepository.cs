using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatform.Infrastructure.Persistence.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _dbContext;

    public ServiceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Services
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Services
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        await _dbContext.Services.AddAsync(service, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Service service, CancellationToken cancellationToken = default)
    {
        _dbContext.Services.Update(service);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Service service, CancellationToken cancellationToken = default)
    {
        _dbContext.Services.Remove(service);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Services.AnyAsync(x => x.Id == id, cancellationToken);
    }
}

using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingPlatform.Infrastructure.Persistence.Repositories;

public class LoyaltyRepository : ILoyaltyRepository
{
    private readonly AppDbContext _dbContext;

    public LoyaltyRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LoyaltyPlan> GetOrCreateActivePlanAsync(CancellationToken cancellationToken = default)
    {
        var plan = await _dbContext.LoyaltyPlans
            .Include(x => x.Levels)
            .Include(x => x.Rewards)
            .FirstOrDefaultAsync(x => x.Activo, cancellationToken);

        if (plan is not null)
        {
            return plan;
        }

        plan = LoyaltyPlan.CreateDefault();

        await _dbContext.LoyaltyPlans.AddAsync(plan, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return plan;
    }

    public async Task<LoyaltyAccount> GetOrCreateAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var plan = await GetOrCreateActivePlanAsync(cancellationToken);

        var account = await _dbContext.LoyaltyAccounts
            .Include(x => x.Redemptions)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (account is not null)
        {
            return account;
        }

        account = new LoyaltyAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = plan.Id,
            Points = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _dbContext.LoyaltyAccounts.AddAsync(account, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return account;
    }

    public async Task<LoyaltyReward?> GetRewardByIdAsync(Guid rewardId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyRewards
            .FirstOrDefaultAsync(x => x.Id == rewardId, cancellationToken);
    }

    public async Task<IReadOnlyList<LoyaltyPlan>> GetPlansAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyPlans
            .AsNoTracking()
            .Include(x => x.Levels)
            .Include(x => x.Rewards)
            .ToListAsync(cancellationToken);
    }

    public async Task<LoyaltyPlan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyPlans
            .Include(x => x.Levels)
            .Include(x => x.Rewards)
            .FirstOrDefaultAsync(x => x.Id == planId, cancellationToken);
    }

    public async Task AddPlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default)
    {
        foreach (var level in plan.Levels)
        {
            level.PlanId = plan.Id;
        }

        await _dbContext.LoyaltyPlans.AddAsync(plan, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.LoyaltyPlans
            .Include(x => x.Levels)
            .FirstOrDefaultAsync(x => x.Id == plan.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Plan de lealtad no encontrado.");

        existing.Nombre = plan.Nombre;
        existing.PuntosPorCita = plan.PuntosPorCita;
        existing.PuntosPorDolar = plan.PuntosPorDolar;
        existing.Activo = plan.Activo;

        _dbContext.LoyaltyLevels.RemoveRange(existing.Levels);

        existing.Levels = plan.Levels
            .Select(level => new LoyaltyLevel
            {
                Id = level.Id,
                PlanId = existing.Id,
                Nombre = level.Nombre,
                MinPoints = level.MinPoints,
                MaxPoints = level.MaxPoints,
                Orden = level.Orden
            })
            .ToList();

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePlanAsync(LoyaltyPlan plan, CancellationToken cancellationToken = default)
    {
        var hasAccounts = await _dbContext.LoyaltyAccounts
            .AnyAsync(x => x.PlanId == plan.Id, cancellationToken);

        if (hasAccounts)
        {
            throw new InvalidOperationException("No se puede eliminar un plan con cuentas de lealtad asociadas.");
        }

        _dbContext.LoyaltyPlans.Remove(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LoyaltyRedemption>> GetRedemptionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyRedemptions
            .AsNoTracking()
            .Include(x => x.Reward)
            .Include(x => x.LoyaltyAccount)
            .Where(x => x.LoyaltyAccount.UserId == userId)
            .OrderByDescending(x => x.RedeemedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyAccounts.CountAsync(cancellationToken);
    }

    public async Task<int> GetTotalRedemptionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.LoyaltyRedemptions.CountAsync(cancellationToken);
    }

    public async Task<decimal> GetAveragePointsPerAccountAsync(CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.LoyaltyAccounts.AnyAsync(cancellationToken))
        {
            return 0m;
        }

        return await _dbContext.LoyaltyAccounts.AverageAsync(x => (decimal)x.Points, cancellationToken);
    }

    public async Task UpdateAccountAsync(LoyaltyAccount account, CancellationToken cancellationToken = default)
    {
        _dbContext.LoyaltyAccounts.Update(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRewardAsync(LoyaltyReward reward, CancellationToken cancellationToken = default)
    {
        _dbContext.LoyaltyRewards.Update(reward);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRedemptionAsync(LoyaltyRedemption redemption, CancellationToken cancellationToken = default)
    {
        await _dbContext.LoyaltyRedemptions.AddAsync(redemption, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
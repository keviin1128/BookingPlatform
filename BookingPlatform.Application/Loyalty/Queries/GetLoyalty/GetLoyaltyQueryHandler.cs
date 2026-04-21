using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Loyalty.DTOs;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Queries.GetLoyalty;

public class GetLoyaltyQueryHandler : IRequestHandler<GetLoyaltyQuery, LoyaltyDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILoyaltyRepository _loyaltyRepository;

    public GetLoyaltyQueryHandler(
        IUserRepository userRepository,
        ILoyaltyRepository loyaltyRepository)
    {
        _userRepository = userRepository;
        _loyaltyRepository = loyaltyRepository;
    }

    public async Task<LoyaltyDto> Handle(GetLoyaltyQuery request, CancellationToken cancellationToken)
    {
        await EnsureAccessAsync(request, cancellationToken);

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var plan = await _loyaltyRepository.GetOrCreateActivePlanAsync(cancellationToken);
        var account = await _loyaltyRepository.GetOrCreateAccountAsync(user.Id, cancellationToken);
        var redemptions = await _loyaltyRepository.GetRedemptionsByUserIdAsync(user.Id, cancellationToken);

        var levels = plan.Levels
            .OrderBy(x => x.Orden)
            .ToList();

        var currentLevel = levels
            .LastOrDefault(level => account.Points >= level.MinPoints)
            ?? levels.First();

        var nextLevel = levels
            .FirstOrDefault(level => level.MinPoints > account.Points);

        var pointsToNextLevel = nextLevel is null
            ? 0
            : Math.Max(0, nextLevel.MinPoints - account.Points);

        var progress = CalculateProgress(account.Points, currentLevel, nextLevel);

        var rewards = plan.Rewards
            .OrderBy(x => x.PuntosRequeridos)
            .ThenBy(x => x.Nombre)
            .Select(reward => LoyaltyRewardDto.FromDomain(reward, account.Points))
            .Where(reward => reward.Disponible)
            .ToList();

        return new LoyaltyDto(
            user.Id,
            account.Points,
            currentLevel.Nombre,
            nextLevel?.Nombre,
            pointsToNextLevel,
            progress,
            rewards,
            redemptions
                .OrderByDescending(x => x.RedeemedAt)
                .Select(LoyaltyRedemptionDto.FromDomain)
                .ToList());
    }

    private static async Task EnsureAccessAsync(GetLoyaltyQuery request, CancellationToken cancellationToken)
    {
        if (request.RequesterRole == Role.Admin)
        {
            return;
        }

        if (request.RequesterRole != Role.Customer || request.RequesterUserId != request.UserId)
        {
            throw new UnauthorizedAccessException("No tienes permisos para ver esta informacion de lealtad.");
        }

        await Task.CompletedTask;
    }

    private static int CalculateProgress(int currentPoints, LoyaltyLevel currentLevel, LoyaltyLevel? nextLevel)
    {
        if (nextLevel is null)
        {
            return 100;
        }

        var range = nextLevel.MinPoints - currentLevel.MinPoints;
        if (range <= 0)
        {
            return 100;
        }

        var progress = (currentPoints - currentLevel.MinPoints) * 100 / range;
        return Math.Clamp(progress, 0, 100);
    }
}
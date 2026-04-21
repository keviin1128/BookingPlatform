using BookingPlatform.Application.Common.Interfaces;
using BookingPlatform.Application.Loyalty.DTOs;
using BookingPlatform.Application.Loyalty.Queries.GetLoyalty;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Commands.RedeemReward;

public class RedeemRewardCommandHandler : IRequestHandler<RedeemRewardCommand, LoyaltyDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILoyaltyRepository _loyaltyRepository;
    private readonly IValidator<RedeemRewardCommand> _validator;

    public RedeemRewardCommandHandler(
        IUserRepository userRepository,
        ILoyaltyRepository loyaltyRepository,
        IValidator<RedeemRewardCommand> validator)
    {
        _userRepository = userRepository;
        _loyaltyRepository = loyaltyRepository;
        _validator = validator;
    }

    public async Task<LoyaltyDto> Handle(RedeemRewardCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        await EnsureAccessAsync(request);

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var plan = await _loyaltyRepository.GetOrCreateActivePlanAsync(cancellationToken);
        var account = await _loyaltyRepository.GetOrCreateAccountAsync(user.Id, cancellationToken);
        var reward = await _loyaltyRepository.GetRewardByIdAsync(request.RewardId, cancellationToken)
            ?? throw new KeyNotFoundException("Recompensa no encontrada.");

        if (reward.PlanId != plan.Id)
        {
            throw new KeyNotFoundException("Recompensa no encontrada.");
        }

        if (!reward.Activo || reward.CantidadDisponible <= 0)
        {
            throw new InvalidOperationException("La recompensa no esta disponible.");
        }

        if (account.Points < reward.PuntosRequeridos)
        {
            throw new InvalidOperationException("No tienes puntos suficientes para canjear la recompensa.");
        }

        account.Points -= reward.PuntosRequeridos;
        account.UpdatedAt = DateTime.UtcNow;
        reward.CantidadDisponible -= 1;

        var redemption = new LoyaltyRedemption
        {
            Id = Guid.NewGuid(),
            LoyaltyAccountId = account.Id,
            RewardId = reward.Id,
            RewardName = reward.Nombre,
            PointsSpent = reward.PuntosRequeridos,
            RedeemedAt = DateTime.UtcNow
        };

        await _loyaltyRepository.UpdateAccountAsync(account, cancellationToken);
        await _loyaltyRepository.UpdateRewardAsync(reward, cancellationToken);
        await _loyaltyRepository.AddRedemptionAsync(redemption, cancellationToken);

        return await BuildResponseAsync(user.Id, cancellationToken);
    }

    private async Task EnsureAccessAsync(RedeemRewardCommand request)
    {
        if (request.RequesterRole == Role.Admin)
        {
            return;
        }

        if (request.RequesterRole != Role.Customer || request.RequesterUserId != request.UserId)
        {
            throw new UnauthorizedAccessException("No tienes permisos para canjear esta recompensa.");
        }

        await Task.CompletedTask;
    }

    private async Task<LoyaltyDto> BuildResponseAsync(Guid userId, CancellationToken cancellationToken)
    {
        var queryHandler = new GetLoyaltyQueryHandler(_userRepository, _loyaltyRepository);
        return await queryHandler.Handle(new GetLoyaltyQuery(userId, userId, Role.Customer), cancellationToken);
    }
}
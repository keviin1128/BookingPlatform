using BookingPlatform.Application.Loyalty.DTOs;
using BookingPlatform.Domain.Enums;
using MediatR;

namespace BookingPlatform.Application.Loyalty.Commands.RedeemReward;

public record RedeemRewardCommand : IRequest<LoyaltyDto>
{
    public Guid UserId { get; init; }

    public Guid RewardId { get; init; }

    public Guid RequesterUserId { get; init; }

    public Role RequesterRole { get; init; }
}
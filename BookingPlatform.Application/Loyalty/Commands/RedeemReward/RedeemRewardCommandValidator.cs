using FluentValidation;

namespace BookingPlatform.Application.Loyalty.Commands.RedeemReward;

public class RedeemRewardCommandValidator : AbstractValidator<RedeemRewardCommand>
{
    public RedeemRewardCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.RewardId)
            .NotEmpty();
    }
}
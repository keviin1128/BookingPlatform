using BookingPlatform.API.Contracts.Loyalty;
using BookingPlatform.Application.Loyalty.Commands.RedeemReward;
using BookingPlatform.Application.Loyalty.DTOs;
using BookingPlatform.Application.Loyalty.Queries.GetLoyalty;
using BookingPlatform.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingPlatform.API.Controllers;

[Route("loyalty")]
[ApiController]
[Authorize(Roles = "Admin,Customer")]
public class LoyaltyController : ControllerBase
{
    private readonly IRequestHandler<GetLoyaltyQuery, LoyaltyDto> _getLoyaltyHandler;
    private readonly IRequestHandler<RedeemRewardCommand, LoyaltyDto> _redeemRewardHandler;

    public LoyaltyController(
        IRequestHandler<GetLoyaltyQuery, LoyaltyDto> getLoyaltyHandler,
        IRequestHandler<RedeemRewardCommand, LoyaltyDto> redeemRewardHandler)
    {
        _getLoyaltyHandler = getLoyaltyHandler;
        _redeemRewardHandler = redeemRewardHandler;
    }

    [HttpGet]
    public async Task<ActionResult<LoyaltyDto>> GetMine()
    {
        var role = GetCurrentRole();
        var requesterUserId = GetCurrentUserId();

        if (role is null || requesterUserId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _getLoyaltyHandler.Handle(
            new GetLoyaltyQuery(requesterUserId.Value, requesterUserId.Value, role.Value),
            HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<LoyaltyDto>> GetByUserId([FromRoute] Guid userId)
    {
        var role = GetCurrentRole();
        var requesterUserId = GetCurrentUserId();

        if (role is null || requesterUserId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _getLoyaltyHandler.Handle(
            new GetLoyaltyQuery(userId, requesterUserId.Value, role.Value),
            HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpPost("redeem")]
    public async Task<ActionResult<LoyaltyDto>> RedeemMine([FromBody] RedeemRewardRequest request)
    {
        var role = GetCurrentRole();
        var requesterUserId = GetCurrentUserId();

        if (role is null || requesterUserId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _redeemRewardHandler.Handle(
            new RedeemRewardCommand
            {
                UserId = requesterUserId.Value,
                RewardId = request.RewardId,
                RequesterUserId = requesterUserId.Value,
                RequesterRole = role.Value
            },
            HttpContext.RequestAborted);

        return Ok(result);
    }

    [HttpPost("{userId:guid}/redeem/{rewardId:guid}")]
    public async Task<ActionResult<LoyaltyDto>> Redeem([FromRoute] Guid userId, [FromRoute] Guid rewardId)
    {
        var role = GetCurrentRole();
        var requesterUserId = GetCurrentUserId();

        if (role is null || requesterUserId is null)
        {
            return Unauthorized(new { message = "No autorizado." });
        }

        var result = await _redeemRewardHandler.Handle(
            new RedeemRewardCommand
            {
                UserId = userId,
                RewardId = rewardId,
                RequesterUserId = requesterUserId.Value,
                RequesterRole = role.Value
            },
            HttpContext.RequestAborted);

        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private Role? GetCurrentRole()
    {
        var value = User.FindFirstValue(ClaimTypes.Role);

        return Enum.TryParse<Role>(value, ignoreCase: true, out var role) ? role : null;
    }
}
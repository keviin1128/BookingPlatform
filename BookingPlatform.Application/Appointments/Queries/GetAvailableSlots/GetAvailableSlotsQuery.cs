using MediatR;

namespace BookingPlatform.Application.Appointments.Queries.GetAvailableSlots;

public record GetAvailableSlotsQuery(
    DateOnly Date,
    Guid WorkerId,
    Guid ServiceId,
    int? Duration) : IRequest<IReadOnlyList<string>>;

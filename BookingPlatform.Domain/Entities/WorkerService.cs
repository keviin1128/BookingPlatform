namespace BookingPlatform.Domain.Entities;

public class WorkerService
{
    public Guid WorkerId { get; set; }

    public Worker Worker { get; set; } = null!;

    public Guid ServiceId { get; set; }

    public Service Service { get; set; } = null!;
}

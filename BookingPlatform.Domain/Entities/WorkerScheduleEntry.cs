namespace BookingPlatform.Domain.Entities;

public class WorkerScheduleEntry
{
    public Guid Id { get; set; }

    public Guid WorkerId { get; set; }

    public Worker Worker { get; set; } = null!;

    public int DayOfWeek { get; set; }

    public string StartTime { get; set; } = null!;

    public string EndTime { get; set; } = null!;
}

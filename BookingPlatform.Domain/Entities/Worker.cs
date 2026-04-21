namespace BookingPlatform.Domain.Entities;

public class Worker
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Especialidad { get; set; } = string.Empty;

    public string HorarioResumen { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public ICollection<WorkerService> WorkerServices { get; set; } = new List<WorkerService>();

    public ICollection<WorkerScheduleEntry> Agenda { get; set; } = new List<WorkerScheduleEntry>();
}

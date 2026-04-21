namespace BookingPlatform.Domain.Entities;

public class Service
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int Duracion { get; set; }

    public decimal Precio { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WorkerService> WorkerServices { get; set; } = new List<WorkerService>();
}

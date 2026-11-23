namespace EmpleadaHogar.Models;

/// <summary>Catálogo configurable de modalidades contractuales disponibles en la aplicación.</summary>
public sealed class ContractTypeDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string OwnerUserId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsIndefinite { get; set; }
    public bool AllowsPartTime { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

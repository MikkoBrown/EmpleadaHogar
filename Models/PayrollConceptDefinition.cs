namespace EmpleadaHogar.Models;

public enum PayrollConceptKind
{
    Earning = 1,
    EmployeeDeduction = 2,
    EmployerContribution = 3,
    EmployerDiscount = 4,
    Information = 5
}

public enum PayrollConceptCalculationType
{
    ManualAmount = 1,
    FixedAmount = 2,
    Percentage = 3,
    HoursTimesRate = 4
}

/// <summary>
/// Catálogo de conceptos de nómina. La definición no sustituye la validación legal:
/// cada concepto debe configurarse con su tratamiento y vigencia antes de usarlo.
/// </summary>
public sealed class PayrollConceptDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string OwnerUserId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public PayrollConceptKind Kind { get; set; }
    public PayrollConceptCalculationType CalculationType { get; set; } = PayrollConceptCalculationType.ManualAmount;
    public decimal? DefaultValue { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsAutomatic { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

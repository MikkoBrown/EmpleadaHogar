namespace EmpleadaHogar.Models;

public sealed class EmployeeProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string OwnerUserId { get; set; }
    public Guid EmployerProfileId { get; set; }
    public EmployerProfile Employer { get; set; } = null!;
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? IdentificationNumber { get; set; }
    public string? SocialSecurityNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public DateOnly? ContractStartDate { get; set; }
    public DateOnly? ContractEndDate { get; set; }
    public required string ContractType { get; set; }
    public decimal WeeklyHours { get; set; }
    // Días ISO 1-7 separados por comas (lunes-domingo).
    public string? WorkDays { get; set; }
    public TimeOnly? WorkStartTime { get; set; }
    public TimeOnly? WorkEndTime { get; set; }
    // Campo heredado para conservar horarios introducidos en versiones anteriores.
    public string? WorkSchedule { get; set; }
    public string? Duties { get; set; }
    public decimal MonthlyBaseSalary { get; set; }
    public decimal ExtraPaymentsMonthly { get; set; }
    public int AnnualVacationDays { get; set; } = 30;
    public decimal IrpfWithholdingPercent { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();
}

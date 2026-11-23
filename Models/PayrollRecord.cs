namespace EmpleadaHogar.Models;

public enum PayrollStatus { Draft, Finalized, Paid, Sent }
public enum PayrollLineKind { Earning, EmployeeDeduction, EmployerContribution }

public sealed class PayrollRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string OwnerUserId { get; set; }
    public Guid EmployeeProfileId { get; set; }
    public EmployeeProfile Employee { get; set; } = null!;
    public required string EmployerNameSnapshot { get; set; }
    public string? EmployerIdentificationSnapshot { get; set; }
    public string? EmployerCccSnapshot { get; set; }
    public string? EmployerAddressSnapshot { get; set; }
    public required string EmployeeNameSnapshot { get; set; }
    public string? EmployeeIdentificationSnapshot { get; set; }
    public string? EmployeeSocialSecuritySnapshot { get; set; }
    public string? EmployeeEmailSnapshot { get; set; }
    public required string ContractTypeSnapshot { get; set; }
    public decimal WeeklyHoursSnapshot { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal GrossPay { get; set; }
    public decimal EmployeeSocialSecurity { get; set; }
    public decimal IrpfWithholding { get; set; }
    public decimal OtherEmployeeDeductions { get; set; }
    public decimal EmployerSocialSecurity { get; set; }
    public decimal NetPay { get; set; }
    public decimal TotalEmployerCost { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public string? PdfStorageKey { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FinalizedUtc { get; set; }
    public DateTime? SentUtc { get; set; }
    public DateTime? PaidUtc { get; set; }
    public ICollection<PayrollLine> Lines { get; set; } = new List<PayrollLine>();
}

public sealed class PayrollLine
{
        public required string OwnerUserId { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PayrollRecordId { get; set; }
    public PayrollRecord Payroll { get; set; } = null!;
    public required string Label { get; set; }
    public PayrollLineKind Kind { get; set; }
    public decimal Amount { get; set; }
}





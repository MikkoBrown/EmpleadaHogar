using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmpleadaHogar.ViewModels;

public sealed class PayrollFormViewModel
{
    [Required]
    public Guid EmployeeProfileId { get; set; }

    [Range(2020, 2100)]
    public int Year { get; set; } = DateTime.Today.Year;

    [Range(1, 12)]
    public int Month { get; set; } = DateTime.Today.Month;

    [Range(0.01, 100000)]
    public decimal GrossPay { get; set; }

    [Range(0, 100000)]
    public decimal EmployeeSocialSecurity { get; set; }

    [Range(0, 100000)]
    public decimal IrpfWithholding { get; set; }

    [Range(0, 100000)]
    public decimal OtherEmployeeDeductions { get; set; }

    [Range(0, 100000)]
    public decimal EmployerSocialSecurity { get; set; }

    public IEnumerable<SelectListItem> Employees { get; set; } = [];

    public Dictionary<Guid, decimal> DefaultGrossByEmployee { get; set; } = [];
}
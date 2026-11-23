using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmpleadaHogar.ViewModels;

public sealed class EmployerFormViewModel
{
    public Guid? Id { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(150)]
    public string LastName { get; set; } = "";

    [StringLength(24, MinimumLength = 0)]
    public string? IdentificationNumber { get; set; }

    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [StringLength(12)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(30)]
    public string? SocialSecurityEmployerCode { get; set; }
}

public sealed class EmployeeFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    public Guid EmployerProfileId { get; set; }

    [Required, StringLength(100)]
    public string FirstName { get; set; } = "";

    [Required, StringLength(150)]
    public string LastName { get; set; } = "";

    [StringLength(24)]
    public string? IdentificationNumber { get; set; }

    [StringLength(24)]
    public string? SocialSecurityNumber { get; set; }

    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }

    [Phone, StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    [RegularExpression("^$|^[0-9]{5}$", ErrorMessage = "El código postal debe tener cinco dígitos.")]
    public string? PostalCode { get; set; }

    [StringLength(100)] public string? City { get; set; }
    [StringLength(100)] public string? Province { get; set; }

    public DateOnly? ContractStartDate { get; set; }

    [Required]
    public string ContractType { get; set; } = "Indefinido";

    [Range(0, 40)]
    public decimal WeeklyHours { get; set; }

    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public bool Saturday { get; set; }
    public bool Sunday { get; set; }

    [DataType(DataType.Time)] public TimeOnly? WorkStartTime { get; set; }
    [DataType(DataType.Time)] public TimeOnly? WorkEndTime { get; set; }
    public string? LegacyWorkSchedule { get; set; }

    [StringLength(500)]
    public string? Duties { get; set; }

    [Range(0, 10000)]
    public decimal MonthlyBaseSalary { get; set; }

    [Range(0, 10000)]
    public decimal ExtraPaymentsMonthly { get; set; }

    [Range(0, 30)]
    public int AnnualVacationDays { get; set; } = 30;

    [Range(0, 100)]
    public decimal IrpfWithholdingPercent { get; set; }

    public IEnumerable<SelectListItem> Employers { get; set; } = [];
}

public sealed class DashboardViewModel
{
    public int EmployerCount { get; set; }

    public int EmployeeCount { get; set; }

    public int PayrollCount { get; set; }

    public int SelectedYear { get; set; }

    public Guid? SelectedEmployerId { get; set; }

    public IEnumerable<SelectListItem> Employers { get; set; } = [];

    public IEnumerable<SelectListItem> AvailableYears { get; set; } = [];

    public List<DashboardMonthlyData> MonthlyData { get; set; } = [];

    public decimal AnnualNetPay { get; set; }

    public decimal AnnualEmployerSocialSecurity { get; set; }

    public decimal AnnualEmployerCost { get; set; }
}

public sealed class DashboardMonthlyData
{
    public int Month { get; set; }

    public string MonthName { get; set; } = "";

    public decimal NetPay { get; set; }

    public decimal EmployerSocialSecurity { get; set; }

    public decimal TotalEmployerCost { get; set; }
}
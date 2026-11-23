using System.Diagnostics;
using System.Globalization;
using EmpleadaHogar.Data;
using EmpleadaHogar.Models;
using EmpleadaHogar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmpleadaHogar.Controllers;

[Authorize]
public sealed class HomeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(
        int? year,
        Guid? employerId,
        CancellationToken cancellationToken)
    {
        var currentYear = DateTime.Today.Year;
        var selectedYear = year ?? currentYear;

        if (selectedYear < 2020 || selectedYear > 2100)
            selectedYear = currentYear;

        var model = new DashboardViewModel
        {
            EmployerCount = await db.Employers.CountAsync(cancellationToken),
            EmployeeCount = await db.Employees.CountAsync(cancellationToken),
            PayrollCount = await db.PayrollRecords.CountAsync(cancellationToken),
            SelectedYear = selectedYear,
            SelectedEmployerId = employerId
        };

        var employers = await db.Employers
            .AsNoTracking()
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => new
            {
                x.Id,
                Name = x.FirstName + " " + x.LastName
            })
            .ToListAsync(cancellationToken);

        model.Employers = new[]
        {
            new SelectListItem("Todos los empleadores", "", employerId is null)
        }.Concat(employers.Select(x =>
            new SelectListItem(x.Name, x.Id.ToString(), employerId == x.Id)));

        var availableYears = await db.PayrollRecords
            .AsNoTracking()
            .Select(x => x.Year)
            .Distinct()
            .OrderByDescending(x => x)
            .ToListAsync(cancellationToken);

        if (!availableYears.Contains(currentYear))
            availableYears.Insert(0, currentYear);

        if (!availableYears.Contains(selectedYear))
            availableYears.Add(selectedYear);

        model.AvailableYears = availableYears
            .Distinct()
            .OrderByDescending(x => x)
            .Select(x => new SelectListItem(
                x.ToString(CultureInfo.InvariantCulture),
                x.ToString(CultureInfo.InvariantCulture),
                x == selectedYear));

        var payrollQuery = db.PayrollRecords
            .AsNoTracking()
            .Where(x =>
                x.Year == selectedYear &&
                x.Status != PayrollStatus.Draft);

        if (employerId.HasValue)
        {
            payrollQuery = payrollQuery.Where(x =>
                x.Employee.EmployerProfileId == employerId.Value);
        }

        var monthlyRecords = await payrollQuery
            .GroupBy(x => x.Month)
            .Select(group => new
            {
                Month = group.Key,
                NetPay = group.Sum(x => x.NetPay),
                EmployerSocialSecurity = group.Sum(x => x.EmployerSocialSecurity),
                TotalEmployerCost = group.Sum(x => x.TotalEmployerCost)
            })
            .ToListAsync(cancellationToken);

        var culture = CultureInfo.GetCultureInfo("es-ES");

        model.MonthlyData = Enumerable.Range(1, 12)
            .Select(month =>
            {
                var data = monthlyRecords.SingleOrDefault(x => x.Month == month);

                return new DashboardMonthlyData
                {
                    Month = month,
                    MonthName = culture.DateTimeFormat.GetAbbreviatedMonthName(month),
                    NetPay = data?.NetPay ?? 0,
                    EmployerSocialSecurity = data?.EmployerSocialSecurity ?? 0,
                    TotalEmployerCost = data?.TotalEmployerCost ?? 0
                };
            })
            .ToList();

        model.AnnualNetPay = model.MonthlyData.Sum(x => x.NetPay);
        model.AnnualEmployerSocialSecurity =
            model.MonthlyData.Sum(x => x.EmployerSocialSecurity);
        model.AnnualEmployerCost =
            model.MonthlyData.Sum(x => x.TotalEmployerCost);

        return View(model);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
}
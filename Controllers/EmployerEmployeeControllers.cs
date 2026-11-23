using EmpleadaHogar.Data;
using EmpleadaHogar.Models;
using EmpleadaHogar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmpleadaHogar.Controllers;

[Authorize]
public sealed class EmployersController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page); pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        var query = db.Employers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(term) || (x.Email != null && x.Email.Contains(term)) || (x.Phone != null && x.Phone.Contains(term)));
        }
        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return View(new PagedResult<EmployerProfile> { Items = items, Page = page, PageSize = pageSize, TotalCount = count, Search = search });
    }

    public IActionResult Create() => View(new EmployerFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployerFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null) return Challenge();
        db.Employers.Add(ToEntity(model, ownerId));
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Empleador/a añadido/a.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var item = await db.Employers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return item is null ? NotFound() : View("Create", FromEntity(item));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EmployerFormViewModel model, CancellationToken cancellationToken)
    {
        var item = await db.Employers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null) return NotFound();
        if (!ModelState.IsValid) { model.Id = id; ViewData["Title"] = "Editar empleador/a"; return View("Create", model); }
        Copy(model, item);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Datos del empleador/a actualizados.";
        return RedirectToAction(nameof(Index));
    }

    private static EmployerProfile ToEntity(EmployerFormViewModel m, string ownerId) => new()
    {
        OwnerUserId = ownerId, FirstName = m.FirstName.Trim(), LastName = m.LastName.Trim(),
        IdentificationNumber = Clean(m.IdentificationNumber), Email = Clean(m.Email), Phone = Clean(m.Phone),
        Address = Clean(m.Address), PostalCode = Clean(m.PostalCode), City = Clean(m.City), Province = Clean(m.Province),
        SocialSecurityEmployerCode = Clean(m.SocialSecurityEmployerCode)
    };

    private static EmployerFormViewModel FromEntity(EmployerProfile x) => new()
    {
        Id = x.Id, FirstName = x.FirstName, LastName = x.LastName, IdentificationNumber = x.IdentificationNumber,
        Email = x.Email, Phone = x.Phone, Address = x.Address, PostalCode = x.PostalCode, City = x.City,
        Province = x.Province, SocialSecurityEmployerCode = x.SocialSecurityEmployerCode
    };

    private static void Copy(EmployerFormViewModel m, EmployerProfile x)
    {
        x.FirstName = m.FirstName.Trim(); x.LastName = m.LastName.Trim(); x.IdentificationNumber = Clean(m.IdentificationNumber);
        x.Email = Clean(m.Email); x.Phone = Clean(m.Phone); x.Address = Clean(m.Address); x.PostalCode = Clean(m.PostalCode);
        x.City = Clean(m.City); x.Province = Clean(m.Province); x.SocialSecurityEmployerCode = Clean(m.SocialSecurityEmployerCode);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

[Authorize]
public sealed class EmployeesController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page); pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        IQueryable<EmployeeProfile> query = db.Employees.AsNoTracking().Include(x => x.Employer);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(term) || (x.Employer.FirstName + " " + x.Employer.LastName).Contains(term));
        }
        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return View(new PagedResult<EmployeeProfile> { Items = items, Page = page, PageSize = pageSize, TotalCount = count, Search = search });
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new EmployeeFormViewModel();
        await LoadEmployers(model, cancellationToken);
        if (!model.Employers.Any()) { TempData["Error"] = "Añade primero un empleador/a."; return RedirectToAction("Create", "Employers"); }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormViewModel model, CancellationToken cancellationToken)
    {
        var employer = await db.Employers.SingleOrDefaultAsync(x => x.Id == model.EmployerProfileId, cancellationToken);
        if (employer is null) ModelState.AddModelError(nameof(model.EmployerProfileId), "Selecciona un empleador/a de tu cuenta.");
        ValidateWorkSchedule(model);
        if (!ModelState.IsValid) { await LoadEmployers(model, cancellationToken); return View(model); }
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null) return Challenge();
        db.Employees.Add(ToEntity(model, ownerId, employer!.Id));
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Trabajadora añadida.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var item = await db.Employees.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null) return NotFound();
        var model = FromEntity(item);
        await LoadEmployers(model, cancellationToken);
        return View("Create", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EmployeeFormViewModel model, CancellationToken cancellationToken)
    {
        var item = await db.Employees.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null) return NotFound();
        var employer = await db.Employers.SingleOrDefaultAsync(x => x.Id == model.EmployerProfileId, cancellationToken);
        if (employer is null) ModelState.AddModelError(nameof(model.EmployerProfileId), "Selecciona un empleador/a de tu cuenta.");
        ValidateWorkSchedule(model);
        if (!ModelState.IsValid) { model.Id = id; await LoadEmployers(model, cancellationToken); ViewData["Title"] = "Editar trabajadora"; return View("Create", model); }
        Copy(model, item, employer!.Id);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Datos de la trabajadora actualizados.";
        return RedirectToAction(nameof(Index));
    }

    private static EmployeeProfile ToEntity(EmployeeFormViewModel m, string ownerId, Guid employerId) => new()
    {
        OwnerUserId = ownerId, EmployerProfileId = employerId, FirstName = m.FirstName.Trim(), LastName = m.LastName.Trim(),
        IdentificationNumber = Clean(m.IdentificationNumber), SocialSecurityNumber = Clean(m.SocialSecurityNumber),
        Email = Clean(m.Email), Phone = Clean(m.Phone), Address = Clean(m.Address), PostalCode = Clean(m.PostalCode), City = Clean(m.City), Province = Clean(m.Province), ContractStartDate = m.ContractStartDate,
        ContractType = m.ContractType, WeeklyHours = CalculateWeeklyHours(m), WorkDays = GetSelectedDays(m), WorkStartTime = m.WorkStartTime, WorkEndTime = m.WorkEndTime, WorkSchedule = BuildScheduleLabel(m), Duties = Clean(m.Duties),
        MonthlyBaseSalary = m.MonthlyBaseSalary, ExtraPaymentsMonthly = m.ExtraPaymentsMonthly,
        AnnualVacationDays = m.AnnualVacationDays, IrpfWithholdingPercent = m.IrpfWithholdingPercent
    };

    private static EmployeeFormViewModel FromEntity(EmployeeProfile x) => new()
    {
        Id = x.Id, EmployerProfileId = x.EmployerProfileId, FirstName = x.FirstName, LastName = x.LastName,
        IdentificationNumber = x.IdentificationNumber, SocialSecurityNumber = x.SocialSecurityNumber, Email = x.Email,
        Phone = x.Phone, Address = x.Address, PostalCode = x.PostalCode, City = x.City, Province = x.Province, ContractStartDate = x.ContractStartDate, ContractType = x.ContractType,
        WeeklyHours = x.WeeklyHours, Monday = HasDay(x.WorkDays, 1), Tuesday = HasDay(x.WorkDays, 2), Wednesday = HasDay(x.WorkDays, 3), Thursday = HasDay(x.WorkDays, 4), Friday = HasDay(x.WorkDays, 5), Saturday = HasDay(x.WorkDays, 6), Sunday = HasDay(x.WorkDays, 7), WorkStartTime = x.WorkStartTime, WorkEndTime = x.WorkEndTime, LegacyWorkSchedule = x.WorkSchedule, Duties = x.Duties,
        MonthlyBaseSalary = x.MonthlyBaseSalary, ExtraPaymentsMonthly = x.ExtraPaymentsMonthly,
        AnnualVacationDays = x.AnnualVacationDays, IrpfWithholdingPercent = x.IrpfWithholdingPercent
    };

    private static void Copy(EmployeeFormViewModel m, EmployeeProfile x, Guid employerId)
    {
        x.EmployerProfileId = employerId; x.FirstName = m.FirstName.Trim(); x.LastName = m.LastName.Trim();
        x.IdentificationNumber = Clean(m.IdentificationNumber); x.SocialSecurityNumber = Clean(m.SocialSecurityNumber);
        x.Email = Clean(m.Email); x.Phone = Clean(m.Phone); x.Address = Clean(m.Address); x.PostalCode = Clean(m.PostalCode); x.City = Clean(m.City); x.Province = Clean(m.Province); x.ContractStartDate = m.ContractStartDate;
        x.ContractType = m.ContractType; x.WeeklyHours = CalculateWeeklyHours(m); x.WorkDays = GetSelectedDays(m); x.WorkStartTime = m.WorkStartTime; x.WorkEndTime = m.WorkEndTime; x.WorkSchedule = BuildScheduleLabel(m); x.Duties = Clean(m.Duties);
        x.MonthlyBaseSalary = m.MonthlyBaseSalary; x.ExtraPaymentsMonthly = m.ExtraPaymentsMonthly;
        x.AnnualVacationDays = m.AnnualVacationDays; x.IrpfWithholdingPercent = m.IrpfWithholdingPercent;
    }

    private void ValidateWorkSchedule(EmployeeFormViewModel model)
    {
        if (!new[] { model.Monday, model.Tuesday, model.Wednesday, model.Thursday, model.Friday, model.Saturday, model.Sunday }.Any(x => x))
            ModelState.AddModelError(nameof(model.Monday), "Selecciona al menos un día de trabajo.");
        if (model.WorkStartTime is null || model.WorkEndTime is null)
            ModelState.AddModelError(nameof(model.WorkStartTime), "Indica la hora de inicio y la hora de fin.");
        else if (model.WorkEndTime <= model.WorkStartTime)
            ModelState.AddModelError(nameof(model.WorkEndTime), "La hora de fin debe ser posterior a la hora de inicio.");
        else if (CalculateWeeklyHours(model) > 40)
            ModelState.AddModelError(nameof(model.WorkEndTime), "El horario supera las 40 horas semanales.");
    }

    private static string? GetSelectedDays(EmployeeFormViewModel m)
    {
        var days = new[] { m.Monday, m.Tuesday, m.Wednesday, m.Thursday, m.Friday, m.Saturday, m.Sunday };
        var selected = days.Select((value, index) => (value, index)).Where(x => x.value).Select(x => (x.index + 1).ToString());
        var result = string.Join(",", selected);
        return result.Length == 0 ? null : result;
    }

    private static bool HasDay(string? days, int day) => (days ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(day.ToString());

    private static decimal CalculateWeeklyHours(EmployeeFormViewModel m) =>
        m.WorkStartTime.HasValue && m.WorkEndTime.HasValue && m.WorkEndTime > m.WorkStartTime
            ? Math.Round((decimal)(m.WorkEndTime.Value - m.WorkStartTime.Value).TotalHours *
                new[] { m.Monday, m.Tuesday, m.Wednesday, m.Thursday, m.Friday, m.Saturday, m.Sunday }.Count(x => x), 2)
            : 0m;

    private static string? BuildScheduleLabel(EmployeeFormViewModel m)
    {
        var names = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
        var flags = new[] { m.Monday, m.Tuesday, m.Wednesday, m.Thursday, m.Friday, m.Saturday, m.Sunday };
        var days = string.Join(", ", names.Where((_, i) => flags[i]));
        return string.IsNullOrWhiteSpace(days) || !m.WorkStartTime.HasValue || !m.WorkEndTime.HasValue
            ? Clean(m.LegacyWorkSchedule)
            : $"{days}: {m.WorkStartTime.Value.ToString("HH:mm")}-{m.WorkEndTime.Value.ToString("HH:mm")}";
    }

    private async Task LoadEmployers(EmployeeFormViewModel model, CancellationToken cancellationToken) =>
        model.Employers = await db.Employers.AsNoTracking().OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem($"{x.FirstName} {x.LastName}", x.Id.ToString(), x.Id == model.EmployerProfileId))
            .ToListAsync(cancellationToken);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

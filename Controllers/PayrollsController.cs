using EmpleadaHogar.Data;
using EmpleadaHogar.Models;
using EmpleadaHogar.Services;
using EmpleadaHogar.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Net.Mail;
using System.Text.Encodings.Web;

namespace EmpleadaHogar.Controllers;

[Authorize]
public sealed class PayrollsController(AppDbContext db, PayslipPdfService pdf, IOutboundEmailService email) : Controller
{
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page); pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        IQueryable<PayrollRecord> query = db.PayrollRecords.AsNoTracking().Include(x => x.Employee).ThenInclude(x => x.Employer);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.EmployeeNameSnapshot.Contains(term) || x.EmployerNameSnapshot.Contains(term));
        }
        var count = await query.CountAsync(cancellationToken);
        var records = await query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return View(new PagedResult<PayrollRecord> { Items = records, Page = page, PageSize = pageSize, TotalCount = count, Search = search });
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new PayrollFormViewModel();
        await LoadEmployees(model, cancellationToken);
        if (!model.Employees.Any())
        {
            TempData["Error"] = "Añade una trabajadora antes de preparar una nómina.";
            return RedirectToAction("Index", "Employees");
        }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PayrollFormViewModel model, CancellationToken cancellationToken)
    {
        var employee = await db.Employees.Include(x => x.Employer)
            .SingleOrDefaultAsync(x => x.Id == model.EmployeeProfileId, cancellationToken);
        if (employee is null) ModelState.AddModelError(nameof(model.EmployeeProfileId), "Selecciona una trabajadora de tu cuenta.");
        if (model.EmployeeSocialSecurity + model.IrpfWithholding + model.OtherEmployeeDeductions > model.GrossPay)
            ModelState.AddModelError(nameof(model.GrossPay), "Las deducciones no pueden superar el salario bruto.");
        if (!ModelState.IsValid)
        {
            await LoadEmployees(model, cancellationToken);
            return View(model);
        }

        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null) return Challenge();
        var netPay = decimal.Round(model.GrossPay - model.EmployeeSocialSecurity - model.IrpfWithholding - model.OtherEmployeeDeductions, 2);
        var record = new PayrollRecord
        {
            OwnerUserId = ownerId, EmployeeProfileId = employee!.Id,
            EmployerNameSnapshot = $"{employee.Employer.FirstName} {employee.Employer.LastName}",
            EmployerIdentificationSnapshot = employee.Employer.IdentificationNumber,
            EmployerCccSnapshot = employee.Employer.SocialSecurityEmployerCode,
            EmployerAddressSnapshot = employee.Employer.Address,
            EmployeeNameSnapshot = $"{employee.FirstName} {employee.LastName}",
            EmployeeIdentificationSnapshot = employee.IdentificationNumber,
            EmployeeSocialSecuritySnapshot = employee.SocialSecurityNumber,
            EmployeeEmailSnapshot = employee.Email,
            ContractTypeSnapshot = employee.ContractType,
            WeeklyHoursSnapshot = employee.WeeklyHours,
            Year = model.Year, Month = model.Month,
            GrossPay = model.GrossPay, EmployeeSocialSecurity = model.EmployeeSocialSecurity,
            IrpfWithholding = model.IrpfWithholding, OtherEmployeeDeductions = model.OtherEmployeeDeductions,
            EmployerSocialSecurity = model.EmployerSocialSecurity, NetPay = netPay,
            TotalEmployerCost = decimal.Round(model.GrossPay + model.EmployerSocialSecurity, 2),
            Status = PayrollStatus.Draft
        };
        record.Lines.Add(new PayrollLine { OwnerUserId = ownerId, Label = "Salario bruto y complementos", Kind = PayrollLineKind.Earning, Amount = model.GrossPay });
        if (model.EmployeeSocialSecurity > 0) record.Lines.Add(new PayrollLine { OwnerUserId = ownerId, Label = "Seguridad Social de la trabajadora", Kind = PayrollLineKind.EmployeeDeduction, Amount = model.EmployeeSocialSecurity });
        if (model.IrpfWithholding > 0) record.Lines.Add(new PayrollLine { OwnerUserId = ownerId, Label = "Retención IRPF", Kind = PayrollLineKind.EmployeeDeduction, Amount = model.IrpfWithholding });
        if (model.OtherEmployeeDeductions > 0) record.Lines.Add(new PayrollLine { OwnerUserId = ownerId, Label = "Otras deducciones", Kind = PayrollLineKind.EmployeeDeduction, Amount = model.OtherEmployeeDeductions });
        if (model.EmployerSocialSecurity > 0) record.Lines.Add(new PayrollLine { OwnerUserId = ownerId, Label = "Cotización a cargo de empleadora", Kind = PayrollLineKind.EmployerContribution, Amount = model.EmployerSocialSecurity });
        db.PayrollRecords.Add(record);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Ya existe una nómina para esa trabajadora y ese mes.");
            await LoadEmployees(model, cancellationToken);
            return View(model);
        }
        TempData["Success"] = "Borrador de nómina guardado. Revisa las cuotas y retenciones antes de finalizarlo.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Pdf(Guid id, CancellationToken cancellationToken)
    {
        var record = await LoadRecord(id, cancellationToken);
        if (record is null) return NotFound();
        var bytes = pdf.Create(record);
        return File(bytes, "application/pdf", $"nomina-{record.Year}-{record.Month:00}.pdf");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalize(Guid id, bool confirmReviewed, CancellationToken cancellationToken)
    {
        var record = await db.PayrollRecords.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (record is null) return NotFound();
        if (!confirmReviewed)
        {
            TempData["Error"] = "Confirma que has revisado el recibo y los importes antes de finalizar.";
            return RedirectToAction(nameof(Index));
        }
        if (record.Status != PayrollStatus.Draft) return RedirectToAction(nameof(Index));
        record.Status = PayrollStatus.Finalized;
        record.FinalizedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Nómina finalizada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(Guid id, CancellationToken cancellationToken)
    {
        var record = await LoadRecord(id, cancellationToken);
        if (record is null) return NotFound();
        if (record.Status != PayrollStatus.Finalized && record.Status != PayrollStatus.Sent)
        {
            TempData["Error"] = "Finaliza y revisa la nómina antes de enviarla.";
            return RedirectToAction(nameof(Index));
        }
        if (string.IsNullOrWhiteSpace(record.Employee.Email))
        {
            TempData["Error"] = "Añade el correo de la trabajadora en su ficha antes de enviar la nómina.";
            return RedirectToAction(nameof(Index));
        }
        var bytes = pdf.Create(record);
        var month = new DateTime(record.Year, record.Month, 1).ToString("MMMM yyyy", CultureInfo.GetCultureInfo("es-ES"));
        try
        {
            await email.SendAsync(record.Employee.Email, $"Recibo de salarios — {month}",
                $"<p>Adjuntamos tu recibo de salarios correspondiente a {HtmlEncoder.Default.Encode(month)}.</p>",
                $"nomina-{record.Year}-{record.Month:00}.pdf", bytes, cancellationToken);
            record.Status = PayrollStatus.Sent;
            record.SentUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            TempData["Success"] = "Nómina enviada al correo de la trabajadora.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or SmtpException or IOException)
        {
            TempData["Error"] = "No se pudo enviar el correo. Revisa la configuración SMTP.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(Guid id, bool confirmPaid, CancellationToken cancellationToken)
    {
        var record = await db.PayrollRecords.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (record is null) return NotFound();
        if (!confirmPaid || record.Status == PayrollStatus.Draft)
        {
            TempData["Error"] = "Finaliza la nómina y confirma que se ha pagado.";
            return RedirectToAction(nameof(Index));
        }
        record.PaidUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Pago registrado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<PayrollRecord?> LoadRecord(Guid id, CancellationToken cancellationToken) =>
        await db.PayrollRecords.AsNoTracking().Include(x => x.Employee).ThenInclude(x => x.Employer)
            .Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    private async Task LoadEmployees(PayrollFormViewModel model, CancellationToken cancellationToken)
    {
        var employees = await db.Employees.AsNoTracking().Include(x => x.Employer)
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(cancellationToken);
        model.Employees = employees.Select(x => new SelectListItem($"{x.FirstName} {x.LastName} — {x.Employer.FirstName} {x.Employer.LastName}", x.Id.ToString(), x.Id == model.EmployeeProfileId)).ToList();
        model.DefaultGrossByEmployee = employees.ToDictionary(x => x.Id, x => x.MonthlyBaseSalary + x.ExtraPaymentsMonthly);
    }
}




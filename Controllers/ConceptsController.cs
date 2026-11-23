using System.Security.Claims;
using EmpleadaHogar.Data;
using EmpleadaHogar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmpleadaHogar.Controllers;

[Authorize]
public sealed class ConceptsController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Contributions(int page = 1, int pageSize = 10, string? search = null, CancellationToken ct = default) => await List(PayrollConceptKind.EmployerContribution, "Cotizaciones", page, pageSize, search, ct);
    public async Task<IActionResult> Deductions(int page = 1, int pageSize = 10, string? search = null, CancellationToken ct = default) => await List(PayrollConceptKind.EmployeeDeduction, "Deducciones", page, pageSize, search, ct);

    private async Task<IActionResult> List(PayrollConceptKind kind, string title, int page, int pageSize, string? search, CancellationToken ct)
    {
        page = Math.Max(1, page); pageSize = pageSize is 10 or 20 or 50 ? pageSize : 10;
        var query = db.PayrollConceptDefinitions.AsNoTracking().Where(x => x.Kind == kind);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search.Trim()) || x.Code.Contains(search.Trim()));
        var count = await query.CountAsync(ct);
        var rows = await query.OrderBy(x => x.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        ViewData["Title"] = title;
        ViewData["Kind"] = kind;
        ViewData["Action"] = kind == PayrollConceptKind.EmployerContribution ? "Contributions" : "Deductions";
        ViewBag.Search = search;
        return View("Index", new ConceptListViewModel { Items = rows, Page = page, PageSize = pageSize, TotalCount = count, Search = search, Kind = kind });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PayrollConceptKind kind, string code, string name, PayrollConceptCalculationType calculationType, decimal? defaultValue, DateOnly validFrom, DateOnly? validTo, CancellationToken ct)
    {
        if (kind is not (PayrollConceptKind.EmployerContribution or PayrollConceptKind.EmployeeDeduction)) return BadRequest();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) ModelState.AddModelError(string.Empty, "Código y nombre son obligatorios.");
        if (validTo.HasValue && validTo.Value < validFrom) ModelState.AddModelError(string.Empty, "La fecha final no puede ser anterior a la inicial.");
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (ownerId is null) return Challenge();
        var duplicate = await db.PayrollConceptDefinitions.AnyAsync(x => x.Code == code.Trim() && x.ValidFrom == validFrom, ct);
        if (duplicate) ModelState.AddModelError(string.Empty, "Ya existe ese código para la fecha de vigencia indicada.");
        if (!ModelState.IsValid) return RedirectToAction(kind == PayrollConceptKind.EmployerContribution ? nameof(Contributions) : nameof(Deductions));
        db.PayrollConceptDefinitions.Add(new PayrollConceptDefinition { OwnerUserId = ownerId, Code = code.Trim(), Name = name.Trim(), Kind = kind, CalculationType = calculationType, DefaultValue = defaultValue, ValidFrom = validFrom, ValidTo = validTo });
        await db.SaveChangesAsync(ct);
        TempData["Success"] = "Concepto guardado.";
        return RedirectToAction(kind == PayrollConceptKind.EmployerContribution ? nameof(Contributions) : nameof(Deductions));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var item = await db.PayrollConceptDefinitions.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound();
        item.IsActive = !item.IsActive;
        await db.SaveChangesAsync(ct);
        return RedirectToAction(item.Kind == PayrollConceptKind.EmployerContribution ? nameof(Contributions) : nameof(Deductions));
    }
}

public sealed class ConceptListViewModel
{
    public IReadOnlyList<PayrollConceptDefinition> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public string? Search { get; init; }
    public PayrollConceptKind Kind { get; init; }
}

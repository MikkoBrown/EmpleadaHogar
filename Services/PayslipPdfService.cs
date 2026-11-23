using System.Globalization;
using EmpleadaHogar.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EmpleadaHogar.Services;

public sealed class PayslipPdfService
{
    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-ES");

    static PayslipPdfService() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] Create(PayrollRecord payroll)
    {
        var employee = payroll.Employee;
        var employer = employee.Employer;
        var period = new DateTime(payroll.Year, payroll.Month, 1).ToString("MMMM yyyy", Spanish);
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(38);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor("#26322E"));
            page.Header().Column(column =>
            {
                column.Item().Text(payroll.Status == PayrollStatus.Draft ? "BORRADOR · NÓMINA MENSUAL" : "RECIBO DE SALARIOS")
                    .FontSize(19).SemiBold().FontColor("#397968");
                column.Item().PaddingTop(5).Text($"Período: {period}").FontSize(12);
                column.Item().PaddingTop(14).LineHorizontal(1).LineColor("#DCE5DF");
            });
            page.Content().PaddingVertical(18).Column(column =>
            {
                column.Spacing(14);
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(x =>
                    {
                        x.Item().Text("EMPLEADORA").FontSize(8).SemiBold().FontColor("#71817A");
                        x.Item().Text(payroll.EmployerNameSnapshot).SemiBold();
                        if (!string.IsNullOrWhiteSpace(payroll.EmployerIdentificationSnapshot)) x.Item().Text($"DNI/NIE: {payroll.EmployerIdentificationSnapshot}");
                        if (!string.IsNullOrWhiteSpace(payroll.EmployerCccSnapshot)) x.Item().Text($"CCC: {payroll.EmployerCccSnapshot}");
                        if (!string.IsNullOrWhiteSpace(payroll.EmployerAddressSnapshot)) x.Item().Text(payroll.EmployerAddressSnapshot);
                    });
                    row.RelativeItem().Column(x =>
                    {
                        x.Item().Text("PERSONA TRABAJADORA").FontSize(8).SemiBold().FontColor("#71817A");
                        x.Item().Text(payroll.EmployeeNameSnapshot).SemiBold();
                        if (!string.IsNullOrWhiteSpace(payroll.EmployeeIdentificationSnapshot)) x.Item().Text($"DNI/NIE: {payroll.EmployeeIdentificationSnapshot}");
                        if (!string.IsNullOrWhiteSpace(payroll.EmployeeSocialSecuritySnapshot)) x.Item().Text($"N.º Seguridad Social: {payroll.EmployeeSocialSecuritySnapshot}");
                        x.Item().Text($"Contrato: {payroll.ContractTypeSnapshot} · {payroll.WeeklyHoursSnapshot:0.##} h/semana");
                    });
                });
                column.Item().Text("DEVENGOS Y DEDUCCIONES").FontSize(9).SemiBold().FontColor("#71817A");
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns => { columns.RelativeColumn(4); columns.RelativeColumn(1.3f); columns.RelativeColumn(1.5f); });
                    table.Header(header =>
                    {
                        HeaderCell(header.Cell(), "Concepto");
                        HeaderCell(header.Cell(), "Tipo");
                        HeaderCell(header.Cell(), "Importe");
                    });
                    foreach (var line in payroll.Lines.OrderBy(x => x.Kind).ThenBy(x => x.Label))
                    {
                        BodyCell(table.Cell(), line.Label);
                        BodyCell(table.Cell(), line.Kind switch { PayrollLineKind.Earning => "Devengo", PayrollLineKind.EmployeeDeduction => "Deducción", _ => "Empresa" });
                        table.Cell().Element(CellStyle).AlignRight().Text(line.Amount.ToString("C", Spanish));
                    }
                });
                column.Item().AlignRight().Column(summary =>
                {
                    summary.Item().Text($"Total devengado: {payroll.GrossPay.ToString("C", Spanish)}");
                    summary.Item().Text($"Total deducciones trabajadora: {(payroll.EmployeeSocialSecurity + payroll.IrpfWithholding + payroll.OtherEmployeeDeductions).ToString("C", Spanish)}");
                    summary.Item().PaddingTop(4).Text($"LÍQUIDO A PERCIBIR: {payroll.NetPay.ToString("C", Spanish)}").FontSize(15).Bold().FontColor("#397968");
                });
                column.Item().PaddingTop(8).Background("#F2F7F4").Padding(12).Column(note =>
                {
                    note.Item().Text($"Cotización empresarial introducida: {payroll.EmployerSocialSecurity.ToString("C", Spanish)}").SemiBold();
                    note.Item().Text($"Coste total estimado: {payroll.TotalEmployerCost.ToString("C", Spanish)}");
                });
                column.Item().PaddingTop(12).Text("Importes guardados según los datos introducidos por la cuenta empleadora. Verifica bases, tipos, retenciones y demás requisitos aplicables antes de entregar el recibo.")
                    .FontSize(8).FontColor("#77837E");
                column.Item().PaddingTop(18).Row(row =>
                {
                    row.RelativeItem().Column(x => { x.Item().LineHorizontal(1).LineColor("#AEBBB4"); x.Item().PaddingTop(5).Text("Firma de la persona trabajadora").FontSize(8); });
                    row.ConstantItem(30);
                    row.RelativeItem().Column(x => { x.Item().LineHorizontal(1).LineColor("#AEBBB4"); x.Item().PaddingTop(5).Text("Firma de la empleadora").FontSize(8); });
                });
            });
            page.Footer().AlignCenter().Text($"EmpleadosHogar · {period}").FontSize(8).FontColor("#97A29D");
        })).GeneratePdf();
    }

    private static void HeaderCell(QuestPDF.Elements.Table.ITableCellContainer cell, string text) =>
        cell.Element(CellStyle).Background("#EDF4F0").PaddingVertical(8).PaddingHorizontal(7).Text(text).SemiBold().FontSize(9);

    private static void BodyCell(QuestPDF.Elements.Table.ITableCellContainer cell, string text) =>
        cell.Element(CellStyle).PaddingVertical(8).PaddingHorizontal(7).Text(text).FontSize(9);

    private static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor("#E8EEEA");
}

public sealed class UnconfiguredEmailService : IOutboundEmailService
{
    public Task SendAsync(string recipient, string subject, string htmlBody, string? attachmentName = null,
        byte[]? attachmentBytes = null, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Configura el servidor SMTP para habilitar el envío de correo.");
}





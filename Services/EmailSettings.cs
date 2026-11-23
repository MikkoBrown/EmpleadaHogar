namespace EmpleadaHogar.Services;

public sealed class EmailSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
    public string FromName { get; set; } = "EmpleadosHogar";
}

public interface IOutboundEmailService
{
    Task SendAsync(string recipient, string subject, string htmlBody, string? attachmentName = null,
        byte[]? attachmentBytes = null, CancellationToken cancellationToken = default);
}

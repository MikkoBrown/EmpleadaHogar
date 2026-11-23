using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using EmpleadaHogar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace EmpleadaHogar.Services;

public sealed class SmtpEmailService(IOptions<EmailSettings> settings) : IOutboundEmailService, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, Microsoft.AspNetCore.Identity.IEmailSender<AppUser>
{
    private readonly EmailSettings _settings = settings.Value;

    public async Task SendAsync(string recipient, string subject, string htmlBody, string? attachmentName = null,
        byte[]? attachmentBytes = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.From))
            throw new InvalidOperationException("Configura las variables SMTP antes de enviar correo.");

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.From, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(recipient));
        if (attachmentBytes is not null && !string.IsNullOrWhiteSpace(attachmentName))
        {
            var stream = new MemoryStream(attachmentBytes, writable: false);
            var attachment = new Attachment(stream, attachmentName, "application/pdf");
            message.Attachments.Add(attachment);
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = string.IsNullOrWhiteSpace(_settings.User)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_settings.User, _settings.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network
        };
        await client.SendMailAsync(message, cancellationToken);
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage) => SendAsync(email, subject, htmlMessage);

    public Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink) =>
        SendAsync(email, "Confirma tu cuenta de EmpleadosHogar",
            $"<p>Confirma tu correo para activar la cuenta:</p><p><a href=\"{HtmlEncoder.Default.Encode(confirmationLink)}\">Confirmar mi correo</a></p>");

    public Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink) =>
        SendAsync(email, "Restablecer contraseña de EmpleadosHogar",
            $"<p>Para crear una contraseña nueva, abre este enlace:</p><p><a href=\"{HtmlEncoder.Default.Encode(resetLink)}\">Restablecer contraseña</a></p>");

    public Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode) =>
        SendAsync(email, "Código para restablecer tu contraseña",
            $"<p>Introduce este código para restablecer tu contraseña:</p><p><strong>{HtmlEncoder.Default.Encode(resetCode)}</strong></p>");
}


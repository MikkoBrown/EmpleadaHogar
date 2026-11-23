using System.ComponentModel.DataAnnotations;
using EmpleadaHogar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace EmpleadaHogar.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender<AppUser> _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;

    public ForgotPasswordModel(
        UserManager<AppUser> userManager,
        IEmailSender<AppUser> emailSender,
        ILogger<ForgotPasswordModel> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required(ErrorMessage = "La dirección de correo es obligatoria.")]
        [EmailAddress(ErrorMessage = "Introduce una dirección de correo válida.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var email = Input.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        // Respuesta neutra para no revelar si una cuenta está registrada.
        if (user is null)
            return RedirectToPage("./ForgotPasswordConfirmation");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code, email },
            protocol: Request.Scheme);

        if (string.IsNullOrWhiteSpace(callbackUrl))
            throw new InvalidOperationException("No se ha podido generar el enlace de recuperación.");

        try
        {
            await _emailSender.SendPasswordResetLinkAsync(user, email, callbackUrl);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "No se pudo enviar el correo de recuperación: SMTP no configurado.");
            ModelState.AddModelError(string.Empty,
                "El servicio de correo todavía no está configurado. Contacta con el administrador de la instalación.");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando el correo de recuperación a {Email}.", email);
            ModelState.AddModelError(string.Empty,
                "No se ha podido enviar el correo. Comprueba la configuración del servicio de correo o inténtalo más tarde.");
            return Page();
        }

        return RedirectToPage("./ForgotPasswordConfirmation");
    }
}

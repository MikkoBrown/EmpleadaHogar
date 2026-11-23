using System.ComponentModel.DataAnnotations;
using EmpleadaHogar.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmpleadaHogar.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<AppUser> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Introduce un correo electrónico válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Content("~/") : returnUrl;
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Content("~/") : returnUrl;
        if (!ModelState.IsValid)
            return Page();

        var email = Input.Email.Trim();
        var result = await _signInManager.PasswordSignInAsync(
            email,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Inicio de sesión correcto para {Email}.", email);
            return LocalRedirect(ReturnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new
            {
                ReturnUrl,
                RememberMe = Input.RememberMe
            });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Cuenta bloqueada para {Email}.", email);
            ModelState.AddModelError(string.Empty,
                "La cuenta está temporalmente bloqueada. Inténtalo de nuevo dentro de 15 minutos.");
            return Page();
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty,
                "Esta cuenta no tiene permitido iniciar sesión. Comprueba la confirmación del correo electrónico.");
            return Page();
        }

        _logger.LogWarning("Intento de inicio de sesión no válido para {Email}.", email);
        ModelState.AddModelError(string.Empty,
            "El correo electrónico o la contraseña no son correctos.");
        return Page();
    }
}

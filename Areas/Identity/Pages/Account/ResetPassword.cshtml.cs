using System.ComponentModel.DataAnnotations;
using System.Text;
using EmpleadaHogar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace EmpleadaHogar.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class ResetPasswordModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;

    public ResetPasswordModel(UserManager<AppUser> userManager) => _userManager = userManager;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Introduce un correo electrónico válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public IActionResult OnGet(string? code = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("No se ha proporcionado el código de recuperación.");

        try
        {
            Input = new InputModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
                Email = email ?? string.Empty
            };
        }
        catch (FormatException)
        {
            return BadRequest("El enlace de recuperación no es válido.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByEmailAsync(Input.Email.Trim());
        if (user is null)
            return RedirectToPage("./ResetPasswordConfirmation");

        var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
        if (result.Succeeded)
            return RedirectToPage("./ResetPasswordConfirmation");

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return Page();
    }
}

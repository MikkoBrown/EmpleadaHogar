using System.ComponentModel.DataAnnotations;
using EmpleadaHogar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmpleadaHogar.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class RegisterModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ILogger<RegisterModel> logger) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ReturnUrl { get; set; }

    public sealed class InputModel
    {
        [Required, StringLength(100)] [Display(Name = "Nombre")]
        public string DisplayName { get; set; } = "";
        [Required, EmailAddress] [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = "";
        [Required, StringLength(100, MinimumLength = 8)] [DataType(DataType.Password)] [Display(Name = "Contraseña")]
        public string Password { get; set; } = "";
        [Required, Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")] [DataType(DataType.Password)] [Display(Name = "Repetir contraseña")]
        public string ConfirmPassword { get; set; } = "";
    }

    public void OnGet(string? returnUrl = null) => ReturnUrl = Url.Content(string.IsNullOrWhiteSpace(returnUrl) ? "~/" : returnUrl);

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = Url.Content(string.IsNullOrWhiteSpace(returnUrl) ? "~/" : returnUrl);
        if (!ModelState.IsValid) return Page();
        var email = Input.Email.Trim();
        var user = new AppUser { UserName = email, Email = email, DisplayName = Input.DisplayName.Trim(), EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            var roleResult = await userManager.AddToRoleAsync(user, "Usuario");
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                foreach (var error in roleResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }
            logger.LogInformation("Se ha creado una cuenta nueva para {Email}.", email);
            await signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(ReturnUrl);
        }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return Page();
    }
}

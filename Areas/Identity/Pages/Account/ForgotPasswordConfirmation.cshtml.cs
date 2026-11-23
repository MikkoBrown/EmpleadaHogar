using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EmpleadaHogar.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class ForgotPasswordConfirmationModel : PageModel
{
    public void OnGet() { }
}

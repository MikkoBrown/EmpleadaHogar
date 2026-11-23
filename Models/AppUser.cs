using Microsoft.AspNetCore.Identity;

namespace EmpleadaHogar.Models;

public sealed class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
}

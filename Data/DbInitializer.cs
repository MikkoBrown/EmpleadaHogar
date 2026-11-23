using Microsoft.AspNetCore.Identity;

namespace EmpleadaHogar.Data;

/// <summary>
/// Inicialización no destructiva: garantiza los roles de la aplicación.
/// No crea, modifica, elimina ni restablece usuarios o contraseñas.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DbInitializer");

        foreach (var roleName in new[] { "Administrador", "Usuario" })
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"No se pudo crear el rol '{roleName}': {errors}");
            }
        }

        logger.LogInformation("Roles de EmpleadaHogar comprobados correctamente.");
    }
}

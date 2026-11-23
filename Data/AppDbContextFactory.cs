using EmpleadaHogar.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EmpleadaHogar.Data;

/// <summary>
/// Crea el contexto para las herramientas de EF Core.
/// Lee la misma configuración que la aplicación y evita usar una conexión ficticia.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true);

        if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
            configurationBuilder.AddUserSecrets<AppDbContextFactory>(optional: true);

        var configuration = configurationBuilder
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se ha configurado ConnectionStrings:DefaultConnection. " +
                "Configura User Secrets o la variable de entorno ConnectionStrings__DefaultConnection antes de ejecutar EF Core.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options, new DesignTimeCurrentUserContext());
    }
}

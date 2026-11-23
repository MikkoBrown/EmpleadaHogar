using EmpleadaHogar.Data;
using EmpleadaHogar.Models;
using EmpleadaHogar.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL: la aplicación y las herramientas de EF Core utilizan
// ConnectionStrings:DefaultConnection.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se ha encontrado ConnectionStrings:DefaultConnection. " +
        "Configura la conexión a PostgreSQL mediante User Secrets o variables de entorno.");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Correo. El servicio se registra siempre para que las páginas de Identity
// puedan resolverse incluso antes de configurar SMTP. El envío informará
// claramente si faltan parámetros.
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddTransient<SmtpEmailService>();
builder.Services.AddTransient<IEmailSender<AppUser>>(sp => sp.GetRequiredService<SmtpEmailService>());
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>(sp => sp.GetRequiredService<SmtpEmailService>());
builder.Services.AddTransient<IOutboundEmailService>(sp => sp.GetRequiredService<SmtpEmailService>());

builder.Services.AddSingleton<PayslipPdfService>();

// ASP.NET Core Identity: mismo patrón que DocuFamilia, conservando AppUser.
builder.Services.AddDefaultIdentity<AppUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

// IMPORTANTE: SameAsRequest permite trabajar en HTTP local (launchSettings)
// y marca la cookie como Secure cuando se accede mediante HTTPS.
// Cookie SecurePolicy.Always impedía mantener la sesión en el perfil HTTP.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "EmpleadosHogar.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddLocalization();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Solo garantiza que existan los roles. No crea, modifica ni elimina usuarios.
await DbInitializer.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization("es-ES", "es");
app.UseRouting();
app.UseAuthentication();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    if (context.User.Identity?.IsAuthenticated == true ||
        context.Request.Path.StartsWithSegments("/Identity"))
    {
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers.Pragma = "no-cache";
    }

    await next();
});

app.UseAuthorization();
app.MapStaticAssets()
    .Add(endpointBuilder => endpointBuilder.Metadata.Add(new AllowAnonymousAttribute()));
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();

using System.Security.Claims;

namespace EmpleadaHogar.Services;

public interface ICurrentUserContext
{
    string? UserId { get; }
}

public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}

using System.Security.Claims;
using DentalERP.SharedKernel.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DentalERP.Modules.IAM.Infrastructure.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Principal?.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    public string Username =>
        Principal?.FindFirstValue(ClaimTypes.Name)
        ?? Principal?.FindFirstValue("unique_name")
        ?? string.Empty;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Permissions =>
        Principal?.FindAll("permission").Select(c => c.Value).ToList() ?? [];

    public bool HasPermission(string permission) =>
        Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}

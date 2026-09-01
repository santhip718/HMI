using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VmcHmi.Application.Interfaces;

namespace VmcHmi.Infrastructure.Auth;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // The user id claim may appear under a full SOAP URI (ClaimTypes.NameIdentifier)
    // or the short JWT name ('nameid') depending on how the inbound claim type map is
    // configured. Resolve either form; the value is a GUID.
    public Guid? UserId =>
        ClaimValue(ClaimTypes.NameIdentifier, JwtRegisteredClaimNames.NameId) is string id &&
        Guid.TryParse(id, out var parsed)
            ? parsed
            : null;

    public string? Username => ClaimValue(JwtRegisteredClaimNames.Sub);

    public string? Role => ClaimValue(ClaimTypes.Role, "role");

    public bool IsAuthenticated => UserId.HasValue;

    private string? ClaimValue(params string[] types)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return null;

        foreach (var type in types)
        {
            var value = user.FindFirst(type)?.Value;
            if (!string.IsNullOrEmpty(value))
                return value;
        }

        return null;
    }
}

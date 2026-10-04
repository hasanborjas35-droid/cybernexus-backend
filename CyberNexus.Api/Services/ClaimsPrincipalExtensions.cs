using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CyberNexus.Api.Services;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The signed-in user's id, or null when the token has no usable id claim.
    /// Accepts both the mapped NameIdentifier and the raw JWT "sub" claim so it
    /// keeps working regardless of the token mapping settings.
    /// </summary>
    public static int? GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(value, out var id) ? id : null;
    }
}

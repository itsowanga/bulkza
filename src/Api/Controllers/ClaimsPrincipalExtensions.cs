using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Controllers;
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetMerchantId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
           ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
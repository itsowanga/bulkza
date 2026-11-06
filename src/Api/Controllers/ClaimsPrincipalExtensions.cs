using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Enums;

namespace Api.Controllers;
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetMerchantId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
           ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    public static MerchantRole? GetMerchantRole(this ClaimsPrincipal user)
    {
        var raw = user.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<MerchantRole>(raw, out var role) ? role : null;
    }
}
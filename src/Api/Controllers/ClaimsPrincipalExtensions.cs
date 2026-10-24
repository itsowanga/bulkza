using System.Security.Claims;

namespace Api.Controllers;
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetMerchantId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirst("sub")?.Value;
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateToken(Merchant merchant)
    {
    
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, merchant.Id.Value.ToString()), 
            new(JwtRegisteredClaimNames.Name, merchant.Name),
            new(ClaimTypes.Role, merchant.Role.ToString()) 
        };


        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
            SecurityAlgorithms.HmacSha256);

      
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            null,
            DateTime.UtcNow.AddHours(_options.ExpirationInHours),
            signingCredentials);


        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
using System.Data;
using System.Security.Authentication;
using Application.Interfaces;
using Domain;
using Domain.Enums;
using Domain.ValueObjects;

namespace Application.Merchants.LoginMerchant;

public class LoginMerchantHandler (IMerchantRepository merchant, IPasswordHasher passwordHasher, IJwtProvider jwt)

{
    private readonly IMerchantRepository _merchant = merchant;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtProvider _jwt = jwt;

    public async Task<LoginMerchantResult> Handle(LoginMerchantCommand cmd, CancellationToken ct)
    {
        var merchant = await _merchant.GetByPhoneNumberAsync(cmd.PhoneNumber, ct) ?? throw new AuthenticationException("Merchant does not exist.");
    
        if(merchant.Status != MerchantStatus.Active) throw new InvalidOperationException("Suspended merchant cannot login. Contact IT.");
        
        var verifyPassword = _passwordHasher.Verify(cmd.PasswordHash.Password, merchant.PasswordHash.Password);

        if(!verifyPassword) throw new AuthenticationException("Invalid password or phone number.");

        var token = _jwt.GenerateToken(merchant);
        return new LoginMerchantResult(token);
    }
}

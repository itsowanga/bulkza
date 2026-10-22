using Application.Merchants.LoginMerchant;
using Application.Merchants.RegisterMerchant;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Merchant;

[ApiController]
[Route("api/merchants")]
public class MerchantController (LoginMerchantHandler loginMerchant, RegisterMerchantHandler registerMerchant) : ControllerBase
{
    private readonly LoginMerchantHandler _loginMerchant = loginMerchant;
    private readonly RegisterMerchantHandler _registerMerchant = registerMerchant;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterMerchant([FromBody] RegisterMerchantRequest request, CancellationToken ct)
    {
        RegisterMerchantCommand command = new(request.Name, PasswordHash.Create(request.Password), Address.Create(  request.Street,
                                                                                                                    request.City,
                                                                                                                    request.Province,
                                                                                                                    request.PostalCode,
                                                                                                                    request.Country,
                                                                                                                    request.Suburb
        ), PhoneNumber.Create(request.PhoneNumber));
        
        var result = await _registerMerchant.Handle(command, ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginMerchant([FromBody] LoginMerchantRequest request, CancellationToken ct)
    {
        LoginMerchantCommand command = new(PhoneNumber.Create(request.PhoneNumber), request.Password);
        var result = await _loginMerchant.Handle(command, ct);
        return Ok(result);
    }

}
using Application.Merchants.LoginMerchant;
using Application.Merchants.RegisterMerchant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/merchants")]
public class MerchantController (LoginMerchantHandler loginMerchant, RegisterMerchantHandler registerMerchant) : ControllerBase
{
    private readonly LoginMerchantHandler _loginMerchant = loginMerchant;
    private readonly RegisterMerchantHandler _registerMerchant = registerMerchant;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterMerchant([FromBody] RegisterMerchantCommand command, CancellationToken ct)
    {
        var result = await _registerMerchant.Handle(command, ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginMerchant([FromBody] LoginMerchantCommand command, CancellationToken ct)
    {
        var result = await _loginMerchant.Handle(command, ct);
        return Ok(result);
    }

}
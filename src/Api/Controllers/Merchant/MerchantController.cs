using Application.Merchants.ChangeMerchantStatus;
using Application.Merchants.LoginMerchant;
using Application.Merchants.RegisterMerchant;
using Application.Merchants.UpdateMerchantAddress;
using Application.Merchants.UpdateMerchantName;
using Application.Merchants.UpdateMerchantPhoneNumber;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Api.Controllers.Merchant;

[ApiController]
[Route("api/merchants")]
public class MerchantController ( LoginMerchantHandler loginMerchant, 
                                  RegisterMerchantHandler registerMerchant,
                                  UpdateMerchantAddressHandler updateAddress,
                                  UpdateMerchantNameHandler updateName,
                                  UpdateMerchantPhoneNumberHandler updatePhoneNumber,
                                  ChangeMerchantStatusHandler changeStatus) : ControllerBase
{
    private readonly LoginMerchantHandler _loginMerchant = loginMerchant;
    private readonly RegisterMerchantHandler _registerMerchant = registerMerchant;
    private readonly UpdateMerchantAddressHandler _updateAddress = updateAddress;
    private readonly UpdateMerchantNameHandler _updateName = updateName;
    private readonly UpdateMerchantPhoneNumberHandler _updatePhoneNumber = updatePhoneNumber;
    private readonly ChangeMerchantStatusHandler _changeStatus = changeStatus;

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

    [Authorize]
    [HttpPut("address/{id}")]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressRequest request, CancellationToken ct)
    {
        UpdateMerchantAddressCommand command = new(MerchantId.Create(id), Address.Create(request.Street,
                                                                                         request.City,
                                                                                         request.Province,
                                                                                         request.PostalCode,
                                                                                         request.Country,
                                                                                         request.Suburb));

        var result = await _updateAddress.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("name/{id}")]
    public async Task<IActionResult> UpdateName(Guid id, [FromBody] UpdateNameRequest request, CancellationToken ct)
    {
        UpdateMerchantNameCommand command = new(MerchantId.Create(id), request.Name);
        var result = await _updateName.Handle(command, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("phone/{id}")]
    public async Task<IActionResult> UpdatePhoneNumber(Guid id, [FromBody] UpdatePhoneNumberRequest request, CancellationToken ct)
    {
        UpdateMerchantPhoneNumberCommand command = new(MerchantId.Create(id), PhoneNumber.Create(request.PhoneNumber));
        var result = await _updatePhoneNumber.Handle(command, ct);
        return Ok(result);
    }
}
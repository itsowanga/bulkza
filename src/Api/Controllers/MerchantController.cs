using Application.Merchants.ChangeMerchantStatus;
using Application.Merchants.CreateMerchant;
using Application.Merchants.UpdateMerchantAddress;
using Application.Merchants.UpdateMerchantName;
using Application.Merchants.UpdateMerchantPhoneNumber;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/merchants")]
public class MerchantController : ControllerBase
{
    private readonly CreateMerchantHandler _createMerchant;
    private readonly ChangeMerchantStatusHandler _changeStatus;
    private readonly UpdateMerchantAddressHandler _updateAddress;
    private readonly UpdateMerchantNameHandler _updateName;
    private readonly UpdateMerchantPhoneNumberHandler _updatePhone;
}
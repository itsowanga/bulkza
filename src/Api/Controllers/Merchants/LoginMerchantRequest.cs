using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Merchants;
public record LoginMerchantRequest
(
    [Required] string PhoneNumber,
    [Required, MinLength(8)] string Password 
);
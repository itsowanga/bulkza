using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Merchant;
public record LoginMerchantRequest
(
    [Required] string PhoneNumber,
    [Required, MinLength(8)] string Password 
);
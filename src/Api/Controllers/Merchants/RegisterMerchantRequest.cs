using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Api.Controllers.Merchants;
public record RegisterMerchantRequest
(
    [Required] string Name,
    [Required] string PhoneNumber,
    [Required, MinLength(8)] string Password,
    [Required] string Street,
    [Required] string City,
    [Required] string Province,
    [Required] string PostalCode,
    [Required] string Country,
    [Optional] string Suburb
);
using System.ComponentModel.DataAnnotations;

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
    string? Suburb = null
);
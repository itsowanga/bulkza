namespace Api.Controllers.Merchant;

public record UpdateAddressRequest(string Street, string City, string PostalCode, string Province, string Country, string? Suburb);
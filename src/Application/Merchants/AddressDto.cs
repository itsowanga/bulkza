namespace Application.Merchants;

public record AddressDto(string Street, string City, string Province, string PostalCode, string Country, string? Suburb);
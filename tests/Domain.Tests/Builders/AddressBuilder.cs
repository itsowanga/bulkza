using Domain.ValueObjects;

namespace Domain.Tests.Builders;
public class AddressBuilder
{
    private string _street = "123 Main St";
    private string? _suburb = null;
    private string _city = "City";
    private string _province = "Province";
    private string _postalCode = "1234";
    private string _country = "Country";

    public AddressBuilder WithStreet(string street) { _street = street; return this; }
    public AddressBuilder WithSuburb(string? suburb) { _suburb = suburb; return this; }
    public AddressBuilder WithCity(string city) { _city = city; return this; }
    public AddressBuilder WithProvince(string province) { _province = province; return this; }
    public AddressBuilder WithPostalCode(string postalCode) { _postalCode = postalCode; return this; }
    public AddressBuilder WithCountry(string country) { _country = country; return this; }

    public Address Build() => Address.Create(_street, _suburb, _city, _province, _postalCode, _country);
}
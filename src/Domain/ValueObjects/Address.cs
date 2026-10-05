namespace Domain.ValueObjects;

public readonly record struct Address
{
    public string Street { get; }
    public string? Suburb { get; }
    public string City { get; }
    public string Province { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address(
        string street,
        string? suburb,
        string city,
        string province,
        string postalCode,
        string country)
    {
        ValidateRequired(street, nameof(street), maxLength: 100);
        ValidateOptional(suburb, nameof(suburb), maxLength: 60);
        ValidateRequired(city, nameof(city), maxLength: 60);
        ValidateRequired(province, nameof(province), maxLength: 60);
        ValidateRequired(postalCode, nameof(postalCode), maxLength: 10);
        ValidateRequired(country, nameof(country), maxLength: 60);

        Street = street;
        Suburb = string.IsNullOrWhiteSpace(suburb) ? null : suburb.Trim();
        City = city;
        Province = province;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(
        string street,
        string city,
        string province,
        string postalCode,
        string country,
        string? suburb = null)
        => new(street, suburb, city, province, postalCode, country);

    private static void ValidateRequired(string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} is required.", fieldName);

        if (value.Length > maxLength)
            throw new ArgumentException($"{fieldName} cannot exceed {maxLength} characters.", fieldName);
    }

    private static void ValidateOptional(string? value, string fieldName, int maxLength)
    {
        if (value is null)
            return;

        if (value.Length > maxLength)
            throw new ArgumentException($"{fieldName} cannot exceed {maxLength} characters.", fieldName);
    }

    public override string ToString()
        => string.IsNullOrEmpty(Suburb)
            ? $"{Street}, {City}, {Province}, {PostalCode}, {Country}"
            : $"{Street}, {Suburb}, {City}, {Province}, {PostalCode}, {Country}";
}
using PhoneNumbers;
using LibPhoneNumber = PhoneNumbers.PhoneNumber;

namespace Domain.ValueObjects;
public readonly record struct PhoneNumber
{
    public string Value {get;}

    private PhoneNumber(string value)
    {

        ArgumentNullException.ThrowIfNull(value);
        
        var phoneUtil = PhoneNumberUtil.GetInstance();

        LibPhoneNumber parsed;
        try
        {
            parsed = phoneUtil.Parse(value, "ZA");
        
        }

        catch(NumberParseException err)
        {
            throw new ArgumentException("Invalid phone number.", nameof(value), err);
        }

        if (!phoneUtil.IsValidNumber(parsed))
        {
           throw new ArgumentException("Entered an invalid phone number for South Africa.", nameof(value));

        }

        Value = phoneUtil.Format(parsed, PhoneNumberFormat.E164);
    } 
    public static PhoneNumber Create(string value) => new(value);

    public override string ToString() => Value;

}
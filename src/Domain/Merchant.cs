using Domain.ValueObjects;

namespace Domain;

public class Merchant
{
    public MerchantId Id {get; private set;}
    public string Name {get; private set;}

    public MerchantStatus Status {get; private set;}
    
    public MerchantRole Role {get; private set;}
    public Address Address {get; private set;}
    public PhoneNumber PhoneNumber {get; private set;}
    public DateTimeOffset RegistrationDate {get; private set;}

    private Merchant() {}

    public Merchant(
        MerchantId id, 
        string name,
        MerchantStatus status,
        MerchantRole role,
        Address address, 
        PhoneNumber phoneNumber,
        DateTimeOffset registrationDate
        )
    {
        NameValidation(name, 100);
        DateValidation(registrationDate);
        Id = id;
        Status = status;
        Role = role;
        Address = address;
        PhoneNumber = phoneNumber; 
    }

    private static void NameValidation(string name, int maxLength)
    {
     
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name should not be blank.");
        }

        if (name.Length > maxLength)
        {
            throw new ArgumentException($"Name should not exceed {maxLength} characters.");

        }
    }
    private static void DateValidation(DateTimeOffset registrationDate)
    {
        if (registrationDate.CompareTo(DateTimeOffset.MinValue) == 0)
        {
            throw new ArgumentException("Registration date is missing.");
        }

        if(registrationDate.CompareTo(DateTimeOffset.Now) == 1)
        {
            throw new ArgumentException("Registration date should not be in the future.");
        }
    }


}

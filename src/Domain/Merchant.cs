using Domain.Enums;
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
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentException("Invalid merchant status.");
        }

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentException("Invalid merchant role.");
        }

        NameValidation(name, 100);
        DateValidation(registrationDate);

        Id = id;
        Status = status;
        Role = role;
        Address = address;
        PhoneNumber = phoneNumber; 
        RegistrationDate = registrationDate;
        Name = name;
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

        if(registrationDate.CompareTo(DateTimeOffset.UtcNow) == 1)
        {
            throw new ArgumentException("Registration date should not be in the future.");
        }
    }

    public void updateName(string name)
    {
        NameValidation(name, 100);
        Name = name;
    }

    public void UpdatePhoneNumber(PhoneNumber phoneNumber)
    {
        if (phoneNumber.Equals(default(PhoneNumber)))
        {
            throw new ArgumentException($"{nameof(phoneNumber)} cannot contain a default phone number.");
        }  

        PhoneNumber = phoneNumber;
    }

    public void UpdateAddress(Address address)
    {
        if (address.Equals(default(Address)))
        {
            throw new ArgumentException($"{nameof(address)} cannot contain a default address.");
        }  

        Address = address;
    }

    public void ChangeMerchantStatus(MerchantStatus status)
    {
        if(!Enum.IsDefined(status))
        {
            throw new ArgumentException($"{nameof(status)} is invalid.");
        }

        Status = status;
    }


}

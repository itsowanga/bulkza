using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Tests.Builders;

public class MerchantBuilder
{

    private MerchantId _id = MerchantId.New();
    private string _name = "abc";

    private PasswordHash _password = PasswordHash.Create("000000000OO00");
    private MerchantStatus _status = MerchantStatus.Active;
    private MerchantRole _role = MerchantRole.Merchant;
    private Address _address = new AddressBuilder().Build();
    private PhoneNumber _phoneNumber = PhoneNumber.Create("0838997415");
    private DateTimeOffset _registrationDate = DateTimeOffset.UtcNow;

    public MerchantBuilder WithMerchantId(MerchantId id)
    {
        _id = id;
        return this;
    }   
    public MerchantBuilder WithMerchantName(string name)
    {
        _name = name;
        return this;
    }
    public MerchantBuilder WithMerchantStatus(MerchantStatus status)
    {
        _status = status;
        return this;
    }
    public MerchantBuilder WithMerchantRole(MerchantRole role)
    {
        _role = role;
        return this;
    }
    public MerchantBuilder WithAddress(Address address)
    {
        _address = address;
        return this;
    }
    public MerchantBuilder WithPhoneNumber(PhoneNumber phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }
    public MerchantBuilder WithRegistrationDate(DateTimeOffset registrationDate)
    {
        _registrationDate = registrationDate;
        return this;
    }

    public Merchant Build() => new(_id, _name, _password,_status, _role, _address, _phoneNumber,_registrationDate);
}
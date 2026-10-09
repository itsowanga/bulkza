using Xunit;
using FluentAssertions;
using Domain.ValueObjects;
using Domain.Enums;
using Domain.Tests.Builders;

namespace Domain.Tests;

public class MerchantTests
{
    [Fact]
    public void
    Constructor_WhenMerchantIdIsTheDefault_ShouldThrowArgumentException()
    {
        MerchantBuilder merchant = new();
        Action act = () => merchant.WithMerchantId(default).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenNameIsEmptyOrNull_ShouldThrowArgumentException()
    {
        MerchantBuilder merchant = new();
        Action act = () => merchant.WithMerchantName(null!).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenNameExceedsTheCharacterLimit_ShouldThrowArgumentException()
    {
        string chars = "abcdefgh";
        MerchantBuilder merchant = new();
        string randomString = new([.. Enumerable.Range(0, 109).Select(_ => chars[Random.Shared.Next(chars.Length)])]);
        Action act = () => merchant.WithMerchantName(randomString).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenMerchantStatusIsNotDefined_ShouldThrowArgumentException()
    {
        MerchantBuilder merchant = new();
        Action act = () => merchant.WithMerchantStatus((MerchantStatus)999).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenMerchantRoleIsNotDefined_ShouldThrowArgumentException()
    {
        MerchantBuilder merchant = new();
        Action act = () => merchant.WithMerchantRole((MerchantRole)999).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenAddressIsTheDefault_ShouldThrowArgumentException()
    {
        MerchantBuilder merchant = new();
        Action act = () => merchant.WithAddress(default).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenPhoneNumberIsTheDefault_ShouldThrowArgumentException()
    {
        MerchantBuilder merchant = new();
        Action act = () => merchant.WithPhoneNumber(default).Build();
        act.Should().Throw<ArgumentException>();
    }


    [Fact]
    public void
    Constructor_WhenRegistrationDateIsInTheFuture_ShouldThrowArgumentException()
    {
        MerchantBuilder merchant = new();
        Action act = () => merchant.WithRegistrationDate(DateTimeOffset.UtcNow.AddMonths(5)).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenRegistrationDateIsTheDefault_ShouldThrowArgumentException()
    {
        MerchantBuilder merchant = new();
        Action act = () => merchant.WithRegistrationDate(default).Build();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Method_WhenUpdatingMerchantAddress_ShouldBeUpdated()
    {
        AddressBuilder address = new();
        MerchantBuilder merchant = new();
        Merchant m1 =  merchant.Build();
        m1.UpdateAddress(address.Build());
        m1.Address.Should().Be(address.Build());
    }  

    [Fact]
    public void
    Method_WhenUpdatingMerchantName_ShouldBeUpdated()
    {
        MerchantBuilder merchant = new();
        Merchant m1 =  merchant.Build();
        m1.UpdateName("aaaaaa");
        m1.Name.Should().Be("aaaaaa");
    }  

    [Fact]
    public void
    Method_WhenUpdatingMerchantPhoneNumber_ShouldBeUpdated()
    {
        MerchantBuilder merchant = new();
        Merchant m1 =  merchant.Build();
        m1.UpdatePhoneNumber(PhoneNumber.Create("0835857878"));
        m1.PhoneNumber.Should().Be(PhoneNumber.Create("0835857878"));
    }  

    [Fact]
    public void
    Method_WhenUpdatingMerchantStatus_ShouldBeUpdated()
    {
        MerchantBuilder merchant = new();
        Merchant m1 =  merchant.Build();
        m1.ChangeMerchantStatus(MerchantStatus.Suspended);
        m1.Status.Should().Be(MerchantStatus.Suspended);
    }    
}

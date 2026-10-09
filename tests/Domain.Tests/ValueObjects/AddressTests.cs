using Xunit;
using FluentAssertions;
using Domain.ValueObjects;

namespace Domain.Tests.ValueObjects;

public class AddressTests
{
    [Theory]
    [InlineData("", "Voortrekker", "Benoni", "Gauteng", "1234", "Zimbabwe")]
    [InlineData("45 Matlala Rd", "Voortrekker", "", "Gauteng", "1234", "Zimbabwe")]
    [InlineData("45 Matlala Rd", "Voortrekker", "Benoni", "", "1234", "Zimbabwe")]
    [InlineData("45 Matlala Rd", "Voortrekker", "Benoni", "Gauteng", "", "Zimbabwe")]
    [InlineData("45 Matlala Rd", "Voortrekker", "Benoni", "Gauteng", "1234", "")]
    public void
    Constructor_WhenFieldIsEmpty_ShouldThrowArgumentException
    ( string street, 
      string? suburb, 
      string city, 
      string province, 
      string postalCode, 
      string country)
    {

        Action act = () => Address.Create(street, city, province, postalCode, country, suburb);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void 
    Constructor_WhenSurburbIsEmpty_ShouldNotThrowArgumentException()
    {
        Action act = () => Address.Create("45 Matlala Rd", "Benoni", "Gauteng", "1234", "Zimbabwe", "");
        act.Should().NotThrow<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenEqualAddressesAreCompared_ShouldBeEqual()
    {
        Address a1 =  Address.Create("45 Matlala Rd", "Benoni", "Gauteng", "1234", "Zimbabwe", "");
        Address a2 =  Address.Create("45 Matlala Rd", "Benoni", "Gauteng", "1234", "Zimbabwe", "");
        a1.Should().Be(a2);
    }
}
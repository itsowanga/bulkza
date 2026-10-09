using Xunit;
using FluentAssertions;
using Domain.ValueObjects;

namespace Domain.Tests.ValueObjects;
public class PhoneNumberTests
{
    [Fact]
    public void
    Constructor_WhenPhoneNumberIsNull_ShouldThrowArgumentNullException()
    {
        Action act = () => PhoneNumber.Create(null!);
        act.Should().Throw<ArgumentNullException>();

    }
    [Fact]
    public void
    Constructor_WhenPhoneNumberIsInvalid_ShouldThrowArgumentException()
    {
        Action act = () => PhoneNumber.Create("abc");
        act.Should().Throw<ArgumentException>();
    }

       [Fact]
    public void
    Constructor_WhenPhoneNumberIsInvalidForRSA_ShouldThrowArgumentException()
    {
        Action act = () => PhoneNumber.Create("01185932155");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("0835851808", "+27385851808")]
    public void
    Constructor_WhenPhoneNumberGetConvertedE164_ShouldBeEqual(string inputNumber, string convertedNumber)
    {
        PhoneNumber phoneNumber = PhoneNumber.Create(inputNumber);
        phoneNumber.ToString().Should().Be(convertedNumber);

    }
}
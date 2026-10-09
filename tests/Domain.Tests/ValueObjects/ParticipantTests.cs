using Xunit;
using FluentAssertions;
using Domain.ValueObjects;

namespace Domain.Tests.ValueObjects;

public class ParticipantTests
{
    [Fact]
    public void
    Constructor_WhenMerchantIdIsDefault_ShouldThrowArgumentException()
    {
        Action act = () => Participant.Create(default(MerchantId), Quantity.Create(150));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void
    Constructor_WhenQuantityIsDefault_ShouldThrowArgumentException()
    {
        Action act = () => Participant.Create(MerchantId.New(), default(Quantity));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void 
    Constructor_WhenEqualParticipantsAreCompared_ShouldBeEqual()
    {
        Guid id = Guid.NewGuid();

        Participant p1 = Participant.Create(MerchantId.Create(id), Quantity.Create(150));
        Participant p2 = Participant.Create(MerchantId.Create(id), Quantity.Create(150));
        p1.Should().Be(p2);
    }
}

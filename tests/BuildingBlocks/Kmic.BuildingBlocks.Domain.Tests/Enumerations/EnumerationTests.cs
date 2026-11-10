using Kmic.BuildingBlocks.Domain.Enumerations;
using Kmic.BuildingBlocks.Domain.Tests.Fakes;
using Shouldly;

namespace Kmic.BuildingBlocks.Domain.Tests.Enumerations;

public sealed class EnumerationTests
{
    [Fact]
    public void All_ContainsEveryValueInDeclarationOrder()
    {
        OrderStatus.All.ShouldBe([OrderStatus.Draft, OrderStatus.Submitted, OrderStatus.Approved, OrderStatus.Rejected]);
    }

    [Fact]
    public void FromCode_WhenUnknown_Throws()
    {
        OrderStatus.FromCode("Submitted").ShouldBeSameAs(OrderStatus.Submitted);
        Should.Throw<InvalidOperationException>(() => OrderStatus.FromCode("Unknown"));
    }

    [Fact]
    public void TryFromCode_WhenUnknownOrWrongCase_ReturnsFalse()
    {
        OrderStatus.TryFromCode("Unknown", out _).ShouldBeFalse();
        OrderStatus.TryFromCode("submitted", out _).ShouldBeFalse();
        OrderStatus.TryFromCode(null, out _).ShouldBeFalse();
        OrderStatus.TryFromCode("Draft", out var draft).ShouldBeTrue();
        draft.ShouldBeSameAs(OrderStatus.Draft);
    }

    [Fact]
    public void Equality_ByCode()
    {
        (OrderStatus.Draft == OrderStatus.FromCode("Draft")).ShouldBeTrue();
        (OrderStatus.Draft != OrderStatus.Submitted).ShouldBeTrue();
        OrderStatus.Draft.ToString().ShouldBe("Draft");
    }

    [Fact]
    public void All_WhenCodesDuplicated_Throws()
    {
        var thrown = Should.Throw<TypeInitializationException>(() => DuplicatedColor.All);

        thrown.InnerException.ShouldBeOfType<InvalidOperationException>();
    }

    private sealed class DuplicatedColor : Enumeration<DuplicatedColor>
    {
        public static readonly DuplicatedColor Red = new("Red", "红");
        public static readonly DuplicatedColor AlsoRed = new("Red", "也是红");

        private DuplicatedColor(string code, string name)
            : base(code, name)
        {
        }
    }
}

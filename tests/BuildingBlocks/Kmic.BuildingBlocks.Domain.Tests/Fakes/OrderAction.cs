using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Domain.Tests.Fakes;

internal sealed class OrderAction : Enumeration<OrderAction>
{
    public static readonly OrderAction Submit = new("Submit", "提交");
    public static readonly OrderAction Approve = new("Approve", "通过");
    public static readonly OrderAction Reject = new("Reject", "驳回");

    private OrderAction(string code, string name)
        : base(code, name)
    {
    }
}

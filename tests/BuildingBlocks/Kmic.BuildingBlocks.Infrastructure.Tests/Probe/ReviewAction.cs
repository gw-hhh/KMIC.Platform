using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class ReviewAction : Enumeration<ReviewAction>
{
    public static readonly ReviewAction Submit = new("Submit", "提交");
    public static readonly ReviewAction Approve = new("Approve", "通过");

    private ReviewAction(string code, string name)
        : base(code, name)
    {
    }
}

using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.Tests.Fakes;

internal static class OrderErrors
{
    public static readonly Error InvalidTransition = Error.BusinessRule("Order.InvalidTransition", "当前状态不允许此操作");

    public static readonly Error NoItems = Error.BusinessRule("Order.NoItems", "没有明细，不能提交");
}

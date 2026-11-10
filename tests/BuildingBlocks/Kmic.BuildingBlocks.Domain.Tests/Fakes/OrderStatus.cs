using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Domain.Tests.Fakes;

internal sealed class OrderStatus : Enumeration<OrderStatus>
{
    public static readonly OrderStatus Draft = new("Draft", "草稿");
    public static readonly OrderStatus Submitted = new("Submitted", "已提交");
    public static readonly OrderStatus Approved = new("Approved", "已通过");
    public static readonly OrderStatus Rejected = new("Rejected", "已驳回");

    private OrderStatus(string code, string name)
        : base(code, name)
    {
    }
}

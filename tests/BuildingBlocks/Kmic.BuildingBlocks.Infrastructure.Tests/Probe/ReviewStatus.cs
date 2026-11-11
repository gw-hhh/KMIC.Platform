using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class ReviewStatus : Enumeration<ReviewStatus>
{
    public static readonly ReviewStatus Draft = new("Draft", "草稿");
    public static readonly ReviewStatus Submitted = new("Submitted", "已提交");
    public static readonly ReviewStatus Approved = new("Approved", "已通过");

    private ReviewStatus(string code, string name)
        : base(code, name)
    {
    }
}

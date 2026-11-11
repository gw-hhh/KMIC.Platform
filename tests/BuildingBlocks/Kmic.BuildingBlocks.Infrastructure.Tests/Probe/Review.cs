using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.StateMachines;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 评审
/// </summary>
/// <remarks>
/// 测试用的有状态聚合：草稿、已提交、已通过。流转会写业务历史
/// </remarks>
internal sealed class Review : AggregateRoot<ReviewId>, IHasRevision
{
    private static readonly StateMachine<Review, ReviewStatus, ReviewAction> _machine =
        StateMachine.Define<Review, ReviewStatus, ReviewAction>(r => r.Status, (r, v) => r.Status = v)
            .Initial(ReviewStatus.Draft)
            .OnInvalid(TicketErrors.Rejected)
            .Permit(ReviewStatus.Draft, ReviewAction.Submit, ReviewStatus.Submitted)
            .Permit(ReviewStatus.Submitted, ReviewAction.Approve, ReviewStatus.Approved)
            .Build();

    private Review(ReviewId id, string title)
        : base(id)
    {
        Title = title;
    }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public ReviewStatus Status { get; private set; } = null!;

    /// <summary>
    /// 业务版本
    /// </summary>
    public int Revision { get; private set; } = 1;

    public static Review Create(string title, string? reason = null)
    {
        var review = new Review(ReviewId.New(), title);
        _machine.Start(review, reason);
        return review;
    }

    public void Rename(string title) => Title = title;

    public Result Submit(string? reason = null) => _machine.Fire(this, ReviewAction.Submit, reason);

    public Result Approve(string? reason = null)
    {
        var fired = _machine.Fire(this, ReviewAction.Approve, reason);
        Revision++;
        return fired;
    }
}

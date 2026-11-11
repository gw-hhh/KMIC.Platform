using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 测试用的聚合：可软删除，有子实体，按所属人与所属组织控制可见范围
/// </summary>
internal sealed class Ticket : AggregateRoot<TicketId>, ISoftDeletable, IHasOwner, IHasOrgUnit
{
    private readonly List<TicketLine> _lines = [];
    private readonly List<Watcher> _watchers = [];

    private Ticket(TicketId id, string number, string title, UserId ownerId, OrgUnitId orgUnitId)
        : base(id)
    {
        Number = number;
        Title = title;
        OwnerId = ownerId;
        OrgUnitId = orgUnitId;
    }

    /// <summary>
    /// 单据编号；不区分大小写，未删除的单据中唯一
    /// </summary>
    public string Number { get; private set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// 所属人
    /// </summary>
    public UserId OwnerId { get; private set; }

    /// <summary>
    /// 所属组织
    /// </summary>
    public OrgUnitId OrgUnitId { get; private set; }

    /// <summary>
    /// 可用额度，单位元
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// 截止时刻；为空表示不限
    /// </summary>
    public DateTimeOffset? DueAt { get; private set; }

    /// <summary>
    /// 备注，长文本
    /// </summary>
    public string? Note { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public TicketStatus Status { get; private set; } = TicketStatus.Open;

    /// <summary>
    /// 优先级
    /// </summary>
    public TicketPriority Priority { get; private set; }

    /// <summary>
    /// 联系邮箱；为空表示未填写
    /// </summary>
    public EmailAddress? ContactEmail { get; private set; }

    /// <summary>
    /// 预算；为空表示未设定
    /// </summary>
    public Money? Budget { get; private set; }

    /// <summary>
    /// 关注人
    /// </summary>
    public IReadOnlyList<Watcher> Watchers => _watchers;

    /// <summary>
    /// 明细行
    /// </summary>
    public IReadOnlyList<TicketLine> Lines => _lines;

    public static Ticket Create(string number, UserId ownerId, OrgUnitId orgUnitId, string title = "测试单据") =>
        new(TicketId.New(), number, title, ownerId, orgUnitId);

    public void Rename(string title) => Title = title;

    public void SetDue(DateTimeOffset dueAt) => DueAt = dueAt;

    public void Fund(decimal amount) => Amount = amount;

    public void Annotate(string note) => Note = note;

    public void Close()
    {
        Status = TicketStatus.Closed;
        Raise(new TicketClosed(Id));
    }

    public void Escalate() => Priority = TicketPriority.Urgent;

    public void ContactAt(EmailAddress email) => ContactEmail = email;

    public void Plan(Money budget) => Budget = budget;

    public void Watch(Watcher watcher) => _watchers.Add(watcher);

    public TicketLine AddLine(string text)
    {
        var line = TicketLine.Create(text);
        _lines.Add(line);
        return line;
    }

    public void ChangeLine(TicketLineId id, string text) => _lines.Single(l => l.Id == id).Change(text);

    public void RemoveLine(TicketLineId id) => _lines.RemoveAll(l => l.Id == id);
}

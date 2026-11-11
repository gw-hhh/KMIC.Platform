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

    public string Number { get; private set; }

    public string Title { get; private set; }

    public UserId OwnerId { get; private set; }

    public OrgUnitId OrgUnitId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTimeOffset? DueAt { get; private set; }

    public string? Note { get; private set; }

    public TicketStatus Status { get; private set; } = TicketStatus.Open;

    public TicketPriority Priority { get; private set; }

    public EmailAddress? ContactEmail { get; private set; }

    public Money? Budget { get; private set; }

    public IReadOnlyList<Watcher> Watchers => _watchers;

    public IReadOnlyList<TicketLine> Lines => _lines;

    public static Ticket Create(string number, UserId ownerId, OrgUnitId orgUnitId, string title = "测试单据") =>
        new(TicketId.New(), number, title, ownerId, orgUnitId);

    public void Rename(string title) => Title = title;

    public void SetDue(DateTimeOffset dueAt) => DueAt = dueAt;

    public void Annotate(string note) => Note = note;

    public void Close() => Status = TicketStatus.Closed;

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

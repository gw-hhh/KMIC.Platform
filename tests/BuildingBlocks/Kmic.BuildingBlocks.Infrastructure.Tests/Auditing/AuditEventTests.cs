using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Auditing;

/// <summary>
/// 审计事件和业务数据同一事务写进 Outbox。每个测试用自己的单据，按单据 ID 找自己的事件
/// </summary>
public sealed class AuditEventTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CreatedByCommand_AddedWithFieldsButNoAuditColumns()
    {
        var id = await OpenAsync("新单据");

        var audit = (await AuditsAsync(id.Value)).ShouldHaveSingleItem();
        audit.Action.ShouldBe("BuildingBlocks.OpenTicket");
        audit.Module.ShouldBe("BuildingBlocks");
        audit.Origin.ShouldBe(CallOrigin.Http.ToString());
        audit.ActorUserId.ShouldBe(_alice.UserId.Value);
        audit.RoleId.ShouldBe(_alice.RoleId.Value);
        audit.OccurredAt.ShouldBe(_harness.Time.GetUtcNow());
        audit.TraceId.Length.ShouldBe(32);
        audit.Targets.ShouldContain(new AuditTarget("Ticket", id.Value));

        var change = audit.Changes.Single(c => string.Equals(c.Entity, "Ticket", StringComparison.Ordinal));
        change.Kind.ShouldBe(ChangeKind.Added);
        change.EntityId.ShouldBe(id.Value);
        var fields = change.Fields.ToDictionary(f => f.Field, f => f.New, StringComparer.Ordinal);
        fields["Title"].ShouldBe("新单据");
        fields["Status"].ShouldBe("Open");
        fields.ContainsKey("CreatedAt").ShouldBeFalse();
        fields.ContainsKey("CreatedBy").ShouldBeFalse();
        fields.ContainsKey("ConcurrencyVersion").ShouldBeFalse();
        fields.ContainsKey("IsDeleted").ShouldBeFalse();
        fields.ContainsKey("Id").ShouldBeFalse();
        change.Fields.ShouldAllBe(f => f.Old == null);
    }

    [Fact]
    public async Task Modified_OnlyChangedFields_WithMaskingAndComplexValues()
    {
        var ticket = await NewTicketAsync();

        (await EditAsync(ticket.Id, t =>
        {
            t.Rename("改名后");
            t.ContactAt(EmailAddress.Create("zhang@example.com").Value);
            t.RotateToken("abc123hash");
            t.Plan(Money.Of(100m, "CNY"));
            t.Watch(Watcher.Of("乙", TicketPriority.Urgent));
        })).ShouldBeSuccess();

        var change = (await AuditsAsync(ticket.Id.Value)).Single().Changes.Single(c => string.Equals(c.Entity, "Ticket", StringComparison.Ordinal));
        change.Kind.ShouldBe(ChangeKind.Modified);
        var fields = change.Fields.ToDictionary(f => f.Field, StringComparer.Ordinal);

        fields["Title"].ShouldBe(new FieldChange("Title", "测试单据", "改名后", Masked: false));
        fields["ContactEmail"].ShouldBe(new FieldChange("ContactEmail", null, "Z***@EXAMPLE.COM", Masked: true));
        fields["AccessTokenHash"].ShouldBe(new FieldChange("AccessTokenHash", null, null, Masked: true));
        fields["Budget.Amount"].New.ShouldBe("100");
        fields["Budget.Currency"].New.ShouldBe("CNY");
        fields["Watchers"].Old.ShouldBe("[]");
        fields["Watchers"].New.ShouldNotBeNull().ShouldContain("乙");
        foreach (var unchanged in new[] { "Number", "Amount", "UpdatedAt", "UpdatedBy", "ConcurrencyVersion" })
        {
            fields.ContainsKey(unchanged).ShouldBeFalse();
        }
    }

    [Fact]
    public async Task ChildEntity_AddedChangedRemoved_EachRecordedWithItsOwnType()
    {
        var ticket = Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]);
        var existing = ticket.AddLine("第一行");
        await _harness.AddAsync(_alice, ticket);

        (await EditAsync(ticket.Id, t => t.AddLine("新增行"))).ShouldBeSuccess();
        (await EditAsync(ticket.Id, t => t.ChangeLine(existing.Id, "改过的行"))).ShouldBeSuccess();
        (await EditAsync(ticket.Id, t => t.RemoveLine(existing.Id))).ShouldBeSuccess();

        var audits = await AuditsAsync(ticket.Id.Value);
        audits.Count.ShouldBe(3);
        audits.ShouldAllBe(a => a.Targets.Count(t => string.Equals(t.ResourceType, "Ticket", StringComparison.Ordinal)) == 1);
        audits.SelectMany(a => a.Changes).Where(c => string.Equals(c.Entity, "TicketLine", StringComparison.Ordinal)).Select(c => c.Kind)
            .ShouldBe([ChangeKind.Added, ChangeKind.Modified, ChangeKind.Deleted]);
    }

    [Fact]
    public async Task SoftDeleteAndRestore_RecordedAsKinds_NoFieldChanges()
    {
        var ticket = await NewTicketAsync();

        (await SendAsync(new EditTicketCommand(ticket.Id, Remove: true))).ShouldBeSuccess();
        (await SendAsync(new EditTicketCommand(ticket.Id, Restore: true))).ShouldBeSuccess();

        var changes = (await AuditsAsync(ticket.Id.Value)).SelectMany(a => a.Changes).Where(c => string.Equals(c.Entity, "Ticket", StringComparison.Ordinal)).ToList();
        changes.Select(c => c.Kind).ShouldBe([ChangeKind.SoftDeleted, ChangeKind.Restored]);
        changes.ShouldAllBe(c => c.Fields.Count == 0);
    }

    [Fact]
    public async Task NoDataChange_StillOneAuditWithDetailsAndTargetAddedByCommand()
    {
        var ticket = await NewTicketAsync();

        (await SendAsync(new EditTicketCommand(ticket.Id, Detail: "导出了 3 行"))).ShouldBeSuccess();

        var audit = (await AuditsAsync(ticket.Id.Value)).ShouldHaveSingleItem();
        audit.Changes.ShouldBeEmpty();
        audit.Details.ShouldBe(new Dictionary<string, string>(StringComparer.Ordinal) { ["detail"] = "导出了 3 行" });
        audit.Targets.ShouldHaveSingleItem().ResourceId.ShouldBe(ticket.Id.Value);
    }

    [Fact]
    public async Task FailedCommand_NoAudit()
    {
        var before = (await AllAuditsAsync()).Count;

        (await SendAsync(new EditTicketCommand(TicketId.New()))).ShouldBeFailure(TicketErrors.NotFound);

        (await AllAuditsAsync()).Count.ShouldBe(before);
    }

    [Fact]
    public async Task SensitiveQuery_Succeeded_OneAuditWithoutChanges()
    {
        var ticket = await NewTicketAsync();

        (await QueryAsync(new ReadTicketDetailQuery(ticket.Id))).ShouldBeSuccess().ShouldBe("测试单据");

        var audit = (await AuditsAsync(ticket.Id.Value)).ShouldHaveSingleItem();
        audit.Action.ShouldBe("BuildingBlocks.ReadTicketDetail");
        audit.ActorUserId.ShouldBe(_alice.UserId.Value);
        audit.Changes.ShouldBeEmpty();
        audit.Details.ShouldBe(new Dictionary<string, string>(StringComparer.Ordinal) { ["filter"] = "只看标题" });
    }

    [Fact]
    public async Task SensitiveQuery_Failed_NoAudit()
    {
        var before = (await AllAuditsAsync()).Count;

        (await QueryAsync(new ReadTicketDetailQuery(TicketId.New()))).ShouldBeFailure(TicketErrors.NotFound);

        (await AllAuditsAsync()).Count.ShouldBe(before);
    }

    [Fact]
    public async Task SensitiveQuery_AuditCannotBeWritten_ResultStillReturned()
    {
        var ticket = await NewTicketAsync();

        (await QueryAsync(new ReadTicketDetailQuery(ticket.Id, TooManyDetails: true))).ShouldBeSuccess().ShouldBe("测试单据");

        (await AuditsAsync(ticket.Id.Value)).ShouldBeEmpty();
    }

    [Fact]
    public async Task LongValue_Truncated_WithHashOfWhole()
    {
        var ticket = await NewTicketAsync();

        (await EditAsync(ticket.Id, t => t.Annotate(new string('长', 2500)))).ShouldBeSuccess();

        var note = (await AuditsAsync(ticket.Id.Value)).Single().Changes.Single().Fields.Single(f => string.Equals(f.Field, "Note", StringComparison.Ordinal));
        note.New.ShouldNotBeNull().ShouldStartWith(new string('长', 1000) + "…(sha256:");
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private async Task<Ticket> NewTicketAsync() =>
        await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));

    private async Task<Result> EditAsync(TicketId id, Action<Ticket> edit) => await SendAsync(new EditTicketCommand(id, edit));

    private async Task<Result> SendAsync(EditTicketCommand command)
    {
        await using var work = _harness.Begin(_alice);
        return await work.Sender.SendAsync(command, Ct);
    }

    private async Task<Result<string>> QueryAsync(ReadTicketDetailQuery query)
    {
        await using var work = _harness.Begin(_alice);
        return await work.Sender.SendAsync(query, Ct);
    }

    private async Task<TicketId> OpenAsync(string title)
    {
        await using var work = _harness.Begin(_alice);
        work.Request.IdempotencyKey = Guid.CreateVersion7().ToString("N");
        return (await work.Sender.SendAsync(new OpenTicketCommand(TicketNumbers.Next(), title), Ct)).ShouldBeSuccess().Id;
    }

    private async Task<List<AuditRecorded>> AuditsAsync(Guid resourceId) =>
        [.. (await AllAuditsAsync()).Where(a => a.Targets.Any(t => t.ResourceId == resourceId))];

    private async Task<List<AuditRecorded>> AllAuditsAsync()
    {
        await using var check = _harness.BeginAsSystem();
        var rows = await check.Db.Set<OutboxMessage>().AsNoTracking()
            .Where(m => m.MessageType == "Framework.AuditRecorded")
            .OrderBy(m => m.Seq)
            .ToListAsync(Ct);
        return [.. rows.Select(r => JsonSerializer.Deserialize<AuditRecorded>(r.Payload, KmicJsonOptions.Stored)!)];
    }
}

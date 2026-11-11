using Kmic.BuildingBlocks.Infrastructure.Maintenance;
using Kmic.BuildingBlocks.Infrastructure.Scheduling;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Messaging;
using Kmic.BuildingBlocks.Testing.Scheduling;
using Kmic.Echo.Probe;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Maintenance;

/// <summary>
/// 过期的数据直接把时间往前改，不等真实时间。每批只删 2 行，几行数据就能走到分批的路径
/// </summary>
public sealed class PurgeTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly DateTimeOffset _at = new(2026, 11, 10, 19, 0, 0, TimeSpan.Zero);

    private readonly TestIdentity _user = TestIdentity.User("统计员").InTree(_org).InOrgUnit(_org["总部"]).WithRole(r => r.Permissions(ProbePermissions.Recount));
    private readonly ProbeHarness _harness = new(database.ConnectionString, services: s => s.Configure<PurgeOptions>(o => o.BatchSize = 2));

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Idempotency_OnlyExpiredDeleted_InBatches()
    {
        var expired = Enumerable.Range(0, 5).Select(_ => Guid.CreateVersion7()).ToList();
        var alive = Guid.CreateVersion7();
        foreach (var id in expired)
        {
            await AddIdempotencyAsync(id, "clock_timestamp() - interval '1 minute'");
        }

        await AddIdempotencyAsync(alive, "clock_timestamp() + interval '1 day'");

        (await _harness.Services.FireAsync("Framework.PurgeIdempotency", _at, Ct)).ShouldBeSuccess().ItemsProcessed.ShouldBe(5);

        (await CountAsync("""SELECT count(*) FROM "Probe"."IdempotencyRecord" WHERE "Id" = ANY(@ids)""", ("ids", expired.ToArray()))).ShouldBe(0);
        (await CountAsync("""SELECT count(*) FROM "Probe"."IdempotencyRecord" WHERE "Id" = @id""", ("id", alive))).ShouldBe(1);
    }

    [Fact]
    public async Task Messages_CredentialKeptWhileSourceDead_RetryNotRunTwice_ThenBothPurged()
    {
        var ticket = await _harness.AddAsync(_user, Ticket.Create(TicketNumbers.Next(), _user.UserId, _org["总部"]));
        await using (var work = _harness.Begin(_user))
        {
            (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, ["清理"]), Ct)).ShouldBeSuccess();
        }

        await _harness.Services.DrainAsync(Ct);
        var message = (await _harness.Services.PublishedAsync<TicketChanged>(Ct)).Single(m => m.Message.Ticket.Id == ticket.Id.Value).Envelope.MessageId;

        // Echo 已经处理完；假设还有一个订阅方一直写不进去，来源行进了死信。两边都早已过了补投窗口
        await ExecuteAsync(
            """
            UPDATE "Probe"."OutboxMessage" SET "Status" = 3, "DeliveredAt" = NULL, "OccurredAt" = "OccurredAt" - interval '30 days' WHERE "Id" = @id;
            UPDATE "Echo"."InboxMessage" SET "ReceivedAt" = "ReceivedAt" - interval '30 days' WHERE "MessageId" = @id;
            """,
            message);

        await PurgeMessagesAsync();
        (await CountAsync("""SELECT count(*) FROM "Probe"."OutboxMessage" WHERE "Id" = @id""", ("id", message))).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Echo"."InboxMessage" WHERE "MessageId" = @id""", ("id", message))).ShouldBe(1);

        // 运维重试死信：去重凭证还在，Echo 不会再执行一遍
        await ExecuteAsync("""UPDATE "Probe"."OutboxMessage" SET "Status" = 0, "NextAttemptAt" = clock_timestamp() WHERE "Id" = @id""", message);
        await _harness.Services.DrainAsync(Ct);
        (await CountAsync("""SELECT count(*) FROM "Probe"."OutboxMessage" WHERE "Id" = @id AND "Status" = 2""", ("id", message))).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Echo"."InboxMessage" WHERE "MessageId" = @id""", ("id", message))).ShouldBe(1);
        (await _harness.Services.PublishedAsync<TicketMirrored>(Ct)).Count(m => m.Message.TicketId == ticket.Id.Value).ShouldBe(1);

        // 投递完成也过了补投窗口：同一次清理里先删来源行，再删凭证
        await ExecuteAsync(
            """UPDATE "Probe"."OutboxMessage" SET "DeliveredAt" = "DeliveredAt" - interval '30 days' WHERE "Id" = @id""",
            message);
        await PurgeMessagesAsync();
        (await CountAsync("""SELECT count(*) FROM "Probe"."OutboxMessage" WHERE "Id" = @id""", ("id", message))).ShouldBe(0);
        (await CountAsync("""SELECT count(*) FROM "Echo"."InboxMessage" WHERE "MessageId" = @id""", ("id", message))).ShouldBe(0);
    }

    [Fact]
    public async Task Messages_WithinWindowOrNotDone_Kept()
    {
        var ticket = await _harness.AddAsync(_user, Ticket.Create(TicketNumbers.Next(), _user.UserId, _org["总部"]));
        await using (var work = _harness.Begin(_user))
        {
            (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, ["保留"]), Ct)).ShouldBeSuccess();
        }

        await _harness.Services.DrainAsync(Ct);
        var message = (await _harness.Services.PublishedAsync<TicketChanged>(Ct)).Single(m => m.Message.Ticket.Id == ticket.Id.Value).Envelope.MessageId;

        // 来源行还在补投窗口内；Echo 的行改成死信并过了窗口，也不自动删
        await ExecuteAsync("""UPDATE "Echo"."InboxMessage" SET "Status" = 3, "ReceivedAt" = "ReceivedAt" - interval '30 days' WHERE "MessageId" = @id""", message);
        await PurgeMessagesAsync();

        (await CountAsync("""SELECT count(*) FROM "Probe"."OutboxMessage" WHERE "Id" = @id""", ("id", message))).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Echo"."InboxMessage" WHERE "MessageId" = @id""", ("id", message))).ShouldBe(1);
    }

    [Fact]
    public async Task Jobs_ExpiredWithoutResultFileDeleted()
    {
        var expired = await FinishedJobAsync("clock_timestamp() - interval '1 minute'", resultFile: false);
        var withFile = await FinishedJobAsync("clock_timestamp() - interval '1 minute'", resultFile: true);
        var alive = await FinishedJobAsync("clock_timestamp() + interval '1 day'", resultFile: false);

        await _harness.Services.FireAsync("Framework.PurgeJobs", _at, Ct);

        (await CountAsync("""SELECT count(*) FROM "Probe"."BackgroundJob" WHERE "Id" = @id""", ("id", expired))).ShouldBe(0);
        (await CountAsync("""SELECT count(*) FROM "Probe"."BackgroundJob" WHERE "Id" = @id""", ("id", withFile))).ShouldBe(1);
        (await CountAsync("""SELECT count(*) FROM "Probe"."BackgroundJob" WHERE "Id" = @id""", ("id", alive))).ShouldBe(1);
    }

    [Fact]
    public async Task ScheduledRuns_StaleRunningAbandoned_OldDeleted()
    {
        var stale = await AddRunAsync(ScheduledRunStatus.Running, "clock_timestamp() - interval '25 hours'", finishedAt: "NULL");
        var running = await AddRunAsync(ScheduledRunStatus.Running, "clock_timestamp() - interval '1 hour'", finishedAt: "NULL");
        var old = await AddRunAsync(ScheduledRunStatus.Succeeded, "clock_timestamp() - interval '40 days'", finishedAt: "clock_timestamp() - interval '40 days'");
        var recent = await AddRunAsync(ScheduledRunStatus.Failed, "clock_timestamp() - interval '2 days'", finishedAt: "clock_timestamp() - interval '2 days'");

        await _harness.Services.FireAsync("Framework.PurgeScheduledJobRuns", _at, Ct);

        (await CountAsync($"""SELECT count(*) FROM {SchedulingTables.Runs} WHERE "Id" = @id AND "Status" = {ScheduledRunStatus.Abandoned}""", ("id", stale))).ShouldBe(1);
        (await CountAsync($"""SELECT count(*) FROM {SchedulingTables.Runs} WHERE "Id" = @id AND "Status" = {ScheduledRunStatus.Running}""", ("id", running))).ShouldBe(1);
        (await CountAsync($"""SELECT count(*) FROM {SchedulingTables.Runs} WHERE "Id" = @id""", ("id", old))).ShouldBe(0);
        (await CountAsync($"""SELECT count(*) FROM {SchedulingTables.Runs} WHERE "Id" = @id""", ("id", recent))).ShouldBe(1);
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private async Task PurgeMessagesAsync() => (await _harness.Services.FireAsync("Framework.PurgeMessages", _at, Ct)).ShouldBeSuccess();

    private async Task AddIdempotencyAsync(Guid id, string expiresAt)
    {
        await using var command = database.DataSource.CreateCommand(
            $"""
            INSERT INTO "Probe"."IdempotencyRecord" ("Id", "UserId", "RoleId", "Operation", "Key", "RequestHash", "Status", "LeaseToken", "LeaseExpiresAt", "CreatedAt", "ExpiresAt")
            VALUES (@id, @id, @id, 'BuildingBlocks.Test', @key, '\x00', 'Completed', @id, clock_timestamp(), clock_timestamp(), {expiresAt})
            """);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("key", id.ToString("N"));
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<Guid> FinishedJobAsync(string expiresAt, bool resultFile)
    {
        var ticket = await _harness.AddAsync(_user, Ticket.Create(TicketNumbers.Next(), _user.UserId, _org["总部"]));
        Guid id;
        await using (var work = _harness.Begin(_user))
        {
            id = (await work.Sender.SendAsync(new StartRecountCommand(ticket.Id), Ct)).ShouldBeSuccess();
        }

        await ExecuteAsync(
            $"""
            UPDATE "Probe"."BackgroundJob" SET "Status" = 2, "FinishedAt" = clock_timestamp(), "ExpiresAt" = {expiresAt},
                "ResultFileId" = {(resultFile ? "@id" : "NULL")}
            WHERE "Id" = @id
            """,
            id);
        return id;
    }

    private async Task<Guid> AddRunAsync(short status, string startedAt, string finishedAt)
    {
        var id = Guid.CreateVersion7();
        await using var command = database.DataSource.CreateCommand(
            $"""
            INSERT INTO {SchedulingTables.Runs} ("Id", "JobName", "ScheduledFireTime", "ActualFireTime", "Trigger", "StartedAt", "FinishedAt", "Status", "InstanceId")
            VALUES (@id, 'BuildingBlocks.DailyStamp', {startedAt}, {startedAt}, 0, {startedAt}, {finishedAt}, @status, 'test')
            """);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("status", status);
        await command.ExecuteNonQueryAsync(Ct);
        return id;
    }

    private async Task ExecuteAsync(string sql, Guid id)
    {
        await using var command = database.DataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<long> CountAsync(string sql, (string Name, object Value) parameter)
    {
        await using var command = database.DataSource.CreateCommand(sql);
        command.Parameters.AddWithValue(parameter.Name, parameter.Value);
        return (long)(await command.ExecuteScalarAsync(Ct))!;
    }
}

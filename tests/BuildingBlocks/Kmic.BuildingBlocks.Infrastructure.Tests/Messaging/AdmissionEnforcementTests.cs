using System.Diagnostics.Metrics;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Metrics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// 容量保护拒绝消息写入：业务事务回滚，返回可重试的 503；软线按来源和命令声明放行
/// </summary>
public sealed class AdmissionEnforcementTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private MessagingAdmission Admission => _harness.Services.GetRequiredService<MessagingAdmission>();

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    [Fact]
    public async Task Normal_WritesTheMessage()
    {
        var ticket = await NewTicketAsync();

        (await ShareAsync(ticket.Id, CallOrigin.Http)).ShouldBeSuccess();

        (await OutboxCountAsync(ticket)).ShouldBe(1);
    }

    [Theory]
    [InlineData(CallOrigin.Http)]
    [InlineData(CallOrigin.Inbox)]
    [InlineData(CallOrigin.BackgroundJob)]
    public async Task HardLine_RejectsEveryOrigin_RollsBack_AndIsRetryable(CallOrigin origin)
    {
        var ticket = await NewTicketAsync();
        Admission.Update(AdmissionState.Hard, TimeSpan.Zero, null);

        var error = (await ShareAsync(ticket.Id, origin)).ShouldBeFailure(CommonErrors.MessagingSuspended);

        error.Metadata[ErrorMetadataKeys.Retryable].ShouldBe(true);
        (await OutboxCountAsync(ticket)).ShouldBe(0);
        (await TitleAsync(ticket)).ShouldBe(ticket.Title, "业务修改要随消息一起回滚");
    }

    [Theory]
    [InlineData(CallOrigin.Http, false)]
    [InlineData(CallOrigin.BackgroundJob, false)]
    [InlineData(CallOrigin.Inbox, true)]
    [InlineData(CallOrigin.ScheduledJob, true)]
    public async Task SoftLine_RejectsHttpAndBackgroundJobOnly(CallOrigin origin, bool allowed)
    {
        var ticket = await NewTicketAsync();
        Admission.Update(AdmissionState.Soft, TimeSpan.Zero, null);

        var result = await ShareAsync(ticket.Id, origin);

        if (allowed)
        {
            result.ShouldBeSuccess();
            (await OutboxCountAsync(ticket)).ShouldBe(1);
        }
        else
        {
            result.ShouldBeFailure(CommonErrors.MessagingSuspended);
            (await OutboxCountAsync(ticket)).ShouldBe(0);
        }
    }

    [Fact]
    public async Task SoftLine_CommandMarkedExempt_StillRuns()
    {
        var ticket = await NewTicketAsync();
        Admission.Update(AdmissionState.Soft, TimeSpan.Zero, null);

        await using var work = _harness.Begin(_alice);
        (await work.Sender.SendAsync(new ShareTicketExemptCommand(ticket.Id, "豁免"), Ct)).ShouldBeSuccess();

        (await OutboxCountAsync(ticket)).ShouldBe(1);
    }

    [Fact]
    public async Task HardLine_CommandMarkedExempt_IsStillRejected()
    {
        var ticket = await NewTicketAsync();
        Admission.Update(AdmissionState.Hard, TimeSpan.Zero, null);

        await using var work = _harness.Begin(_alice);
        (await work.Sender.SendAsync(new ShareTicketExemptCommand(ticket.Id, "豁免"), Ct)).ShouldBeFailure(CommonErrors.MessagingSuspended);

        (await OutboxCountAsync(ticket)).ShouldBe(0);
    }

    [Fact]
    public async Task Recovered_WritesAgainInTheSameScopeFactory()
    {
        var ticket = await NewTicketAsync();
        Admission.Update(AdmissionState.Hard, TimeSpan.Zero, null);
        (await ShareAsync(ticket.Id, CallOrigin.Http)).ShouldBeFailure(CommonErrors.MessagingSuspended);

        Admission.Update(AdmissionState.Normal, TimeSpan.Zero, null);

        (await ShareAsync(ticket.Id, CallOrigin.Http)).ShouldBeSuccess();
        (await OutboxCountAsync(ticket)).ShouldBe(1);
    }

    [Fact]
    public async Task EveryRejectedWrite_IsCountedByStateAndOrigin()
    {
        using var metrics = new MetricRecorder(_harness.Services.GetRequiredService<IMeterFactory>());
        var ticket = await NewTicketAsync();

        Admission.Update(AdmissionState.Soft, TimeSpan.Zero, null);
        await ShareAsync(ticket.Id, CallOrigin.Http);
        await ShareAsync(ticket.Id, CallOrigin.Http);
        Admission.Update(AdmissionState.Hard, TimeSpan.Zero, null);
        await ShareAsync(ticket.Id, CallOrigin.Inbox);

        var counted = metrics.Of("kmic.messaging.admission.rejected");
        counted.Where(m => HasTags(m, "soft", "Http", "rejected")).Sum(m => m.Value).ShouldBe(2);
        counted.Where(m => HasTags(m, "hard", "Inbox", "rejected")).Sum(m => m.Value).ShouldBe(1);
    }

    [Fact]
    public async Task AWriteThatIsAllowed_IsNotCounted()
    {
        using var metrics = new MetricRecorder(_harness.Services.GetRequiredService<IMeterFactory>());
        var ticket = await NewTicketAsync();

        (await ShareAsync(ticket.Id, CallOrigin.Http)).ShouldBeSuccess();

        metrics.Of("kmic.messaging.admission.rejected").ShouldBeEmpty();
    }

    private static bool HasTags(RecordedMeasurement measurement, string state, string origin, string outcome) =>
        string.Equals(measurement.Tags["state"], state, StringComparison.Ordinal)
        && string.Equals(measurement.Tags["origin"], origin, StringComparison.Ordinal)
        && string.Equals(measurement.Tags["outcome"], outcome, StringComparison.Ordinal);

    private Task<Ticket> NewTicketAsync() => _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));

    private async Task<Result> ShareAsync(TicketId id, CallOrigin origin)
    {
        await using var work = _harness.Begin(_alice, origin);
        return await work.Sender.SendAsync(new ShareTicketCommand(id, ["新标题"]), Ct);
    }

    private async Task<int> OutboxCountAsync(Ticket ticket)
    {
        await using var check = _harness.BeginAsSystem();
        return await check.Db.Set<OutboxMessage>().AsNoTracking().CountAsync(m => m.SourceId == ticket.Id.Value, Ct);
    }

    private async Task<string> TitleAsync(Ticket ticket)
    {
        await using var check = _harness.BeginAsSystem();
        return (await check.Db.Set<Ticket>().AsNoTracking().SingleAsync(t => t.Id == ticket.Id, Ct)).Title;
    }
}

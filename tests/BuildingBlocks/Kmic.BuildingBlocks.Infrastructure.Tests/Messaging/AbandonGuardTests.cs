using System.Text.Json;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// 放弃之前的检查：按消息名找到类型，在系统身份的作用域里取接收方注册的检查，没有注册的直接放行，看不懂的消息不放行
/// </summary>
public sealed class AbandonGuardTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly Error _refused = Error.BusinessRule("Test.StillAttached", "引用的文件还没有归属，只能重试");

    private readonly RecordingPolicy _policy = new();
    private ProbeHarness? _harness;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private IAbandonGuard Guard => (_harness ??= new ProbeHarness(database.ConnectionString, services: s => s.AddSingleton<IAbandonPolicy<TicketChanged>>(_policy)))
        .Services.GetRequiredService<IAbandonGuard>();

    public ValueTask DisposeAsync() => _harness?.DisposeAsync() ?? ValueTask.CompletedTask;

    private static DeadMessageDetail Detail(string type, string payload, int version = 1) =>
        new(
            new DeadMessage(Guid.CreateVersion7(), MessageBox.Inbox, "Echo", "Echo.MirrorTicket", Guid.CreateVersion7(), type, version, DateTimeOffset.UnixEpoch, 10, "失败"),
            DeadMessageState.Dead,
            payload,
            "{}");

    private static string TicketPayload(string title) => JsonSerializer.Serialize(new TicketChanged(new TicketSnapshot(Guid.CreateVersion7(), title, "Open")), KmicJsonOptions.Stored);

    [Fact]
    public async Task APolicyForTheMessage_IsCalledWithTheRestoredPayload_AndItsAnswerIsTheAnswer()
    {
        _policy.Verdict = Result.Success();

        (await Guard.CheckAsync(Detail("BuildingBlocks.TicketChanged", TicketPayload("第一张")), Ct)).ShouldBeSuccess();
        _policy.Verdict = _refused;
        var refused = await Guard.CheckAsync(Detail("BuildingBlocks.TicketChanged", TicketPayload("第二张")), Ct);

        refused.ShouldBeFailure(_refused);
        _policy.Seen.Select(t => t.Ticket.Title).ShouldBe(["第一张", "第二张"]);
    }

    [Fact]
    public async Task AMessageWithoutAPolicy_CanBeAbandonedAtOnce()
    {
        // 没有为 TicketClosedNotice 注册检查：放弃之前没有什么要核对的
        var detail = Detail("BuildingBlocks.TicketClosedNotice", """{"ticketId":"6f2a0f6e-0f64-4d8e-9a53-3a2b45c0b7aa"}""");

        var result = await Guard.CheckAsync(detail, Ct);

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task AMessageTheCatalogDoesNotKnow_IsNotAbandoned_AndNeitherIsOneWhosePayloadCannotBeRead()
    {
        _policy.Verdict = Result.Success();

        var unknownType = await Guard.CheckAsync(Detail("BuildingBlocks.NoSuchMessage", "{}"), Ct);
        var unknownVersion = await Guard.CheckAsync(Detail("BuildingBlocks.TicketChanged", TicketPayload("x"), version: 99), Ct);
        var unreadable = await Guard.CheckAsync(Detail("BuildingBlocks.TicketChanged", """{"ticket":"不是对象"}"""), Ct);
        var notJson = await Guard.CheckAsync(Detail("BuildingBlocks.TicketChanged", "不是 JSON"), Ct);

        unknownType.ShouldBeFailure(CommonErrors.MessageUnknown);
        unknownVersion.ShouldBeFailure(CommonErrors.MessageUnknown);
        unreadable.ShouldBeFailure(CommonErrors.MessagePayloadUnreadable);
        notJson.ShouldBeFailure(CommonErrors.MessagePayloadUnreadable);
        _policy.Seen.ShouldBeEmpty("看不懂的消息连检查都不调用");
    }

    private sealed class RecordingPolicy : IAbandonPolicy<TicketChanged>
    {
        public Result Verdict { get; set; } = Result.Success();

        public List<TicketSnapshotView> Seen { get; } = [];

        public ValueTask<Result> CheckAsync(TicketChanged message, CancellationToken ct)
        {
            Seen.Add(new TicketSnapshotView(message.Ticket));
            return ValueTask.FromResult(Verdict);
        }
    }

    private sealed record TicketSnapshotView(TicketSnapshot Ticket);
}

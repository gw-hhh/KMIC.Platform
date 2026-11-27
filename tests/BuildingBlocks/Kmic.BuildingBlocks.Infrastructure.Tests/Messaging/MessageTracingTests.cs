using System.Collections.Concurrent;
using System.Diagnostics;
using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

// ActivityListener 为进程级全局监听，按原请求 TraceId 精确匹配目标链路
public sealed class MessageTracingTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private const string TestSource = "Kmic.Tests.Tracing";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);
    private readonly ConcurrentQueue<Activity> _started = new();
    private readonly ActivitySource _tests = new(TestSource);
    private ActivityListener? _listener;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => string.Equals(source.Name, KmicActivities.MessagingName, StringComparison.Ordinal) || string.Equals(source.Name, TestSource, StringComparison.Ordinal),
            Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = _started.Enqueue,
        };
        ActivitySource.AddActivityListener(_listener);

        // 预先注册订阅路由，确保发布的消息能够投递至 Echo 模块
        var catalog = _harness.Services.GetRequiredService<SubscriptionCatalog>();
        await new MessageRoutes(database.DataSource).RegisterAsync([.. MessageRoutes.Declared(catalog)], [], "test", Ct);
    }

    [Fact]
    public async Task InboxProcessing_StartsANewTraceLinkedToThePublishingRequest()
    {
        var request = await PublishInRequestAsync("追踪链接");

        await DrainAsync();

        var inbox = InboxActivityLinkedTo(request);
        inbox.ParentId.ShouldBeNull("消息消费应开启独立 Trace，不应作为原请求的子 Span");
        inbox.TraceId.ShouldNotBe(request.TraceId);
    }

    [Fact]
    public async Task InboxProcessing_TheCommandSpanBelongsToTheInboxTrace()
    {
        var request = await PublishInRequestAsync("命令在同一追踪里");

        await DrainAsync();

        var inbox = InboxActivityLinkedTo(request);
        _started.ShouldContain(a => a.ParentSpanId == inbox.SpanId && a.TraceId == inbox.TraceId);
    }

    public async ValueTask DisposeAsync()
    {
        _listener?.Dispose();
        _tests.Dispose();
        await _harness.DisposeAsync();
    }

    // 在独立的测试请求 Activity 上下文中发布消息并返回该 Activity
    private async Task<Activity> PublishInRequestAsync(string title)
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));
        using var request = _tests.StartActivity("发布消息的请求") ?? throw new InvalidOperationException("没有监听器，活动没有创建。");
        await using (var work = _harness.Begin(_alice))
        {
            (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, [title]), Ct)).ShouldBeSuccess();
        }

        return request;
    }

    private Activity InboxActivityLinkedTo(Activity request)
    {
        var found = _started.Where(a => a.OperationName.StartsWith("inbox ", StringComparison.Ordinal) && a.Links.Any(l => l.Context.TraceId == request.TraceId)).ToList();
        if (found.Count != 1)
        {
            var all = string.Join("; ", _started.Select(a => $"{a.OperationName} trace={a.TraceId} parent={a.ParentSpanId} links={string.Join(',', a.Links.Select(l => l.Context.TraceId))}"));
            throw new InvalidOperationException($"request trace={request.TraceId}，找到 {found.Count} 个。全部：{all}");
        }

        return found[0];
    }

    private async Task DrainAsync()
    {
        var dispatcher = _harness.Services.GetRequiredService<OutboxDispatcher>();
        while (await dispatcher.DeliverBatchAsync("BuildingBlocks", Ct) > 0)
        {
        }

        var processor = _harness.Services.GetRequiredService<InboxProcessor>();
        while (await processor.ProcessBatchAsync("Echo", Ct) > 0)
        {
        }
    }
}

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 把本次命令发布的消息写进本模块的 Outbox
/// </summary>
/// <remarks>
/// 在业务数据保存之后、提交之前调用：快照事件要带上版本号加一之后的值
/// </remarks>
internal sealed class OutboxWriter(
    IntegrationEventPublisher publisher,
    IExecutionContext execution,
    TimeProvider time,
    IOptions<MessagingOptions> options)
{
    public void Discard() => publisher.Discard();

    public async Task WriteAsync(ModuleDbContext context, string module, CancellationToken ct)
    {
        var pending = publisher.TakePending();
        if (pending.Count == 0)
        {
            return;
        }

        var limits = options.Value;
        var now = time.GetUtcNow();
        var headers = JsonSerializer.Serialize(Headers(module, execution.User), KmicJsonOptions.Stored);
        var versions = AggregateVersions(context);
        var total = 0;
        foreach (var message in LatestSnapshots(pending))
        {
            var payload = JsonSerializer.Serialize(message.Message, message.Type.ClrType, KmicJsonOptions.Stored);
            var size = Encoding.UTF8.GetByteCount(payload);
            if (size > limits.MaxPayloadBytes)
            {
                throw new InvalidOperationException(
                    $"{message.Type.Name} 的内容有 {size} 字节，超过上限 {limits.MaxPayloadBytes}。大块内容请存成文件，消息里只放文件 ID。");
            }

            total += size;
            var sourceId = (message.Message as ISnapshotEvent)?.SourceId;
            context.Set<OutboxMessage>().Add(new OutboxMessage
            {
                Id = Guid.CreateVersion7(),
                MessageType = message.Type.Name,
                SchemaVersion = message.Type.Version,
                Kind = message.Type.IsRequest ? (short)1 : (short)0,
                Payload = payload,
                Headers = headers,
                SourceId = sourceId,
                SourceVersion = sourceId is { } id ? VersionOf(versions, message.Type, id) : null,
                OrderingKey = message.Type.OrderingKeyOf(message.Message) ?? sourceId?.ToString(),
                SubjectIds = message.Type.SubjectIdsOf(message.Message)?.ToArray(),
                OccurredAt = now,
                Status = MessageStatus.Pending,
            });
        }

        if (total > limits.MaxOutboxBytesPerTransaction)
        {
            throw new InvalidOperationException(
                $"这个命令要写入 Outbox {total} 字节，超过一个事务的上限 {limits.MaxOutboxBytesPerTransaction}。大批量的消息请拆到后台作业里分批发布。");
        }

        await context.SaveChangesAsync(ct);

        // 通知在提交时才送达，投递程序被唤醒时一定能读到这些消息
        await context.Database.ExecuteSqlAsync($"SELECT pg_notify({MessagingChannels.Outbox}, {module})", ct);
    }

    private static MessageHeaders Headers(string module, ICurrentUser user)
    {
        var activity = Activity.Current;
        return new MessageHeaders(
            module,
            (user.IsSystem ? user.OnBehalfOf : user.UserId)?.Value,
            user.ActualUserId?.Value,
            CausationId: null,
            activity?.Id,
            activity?.TraceStateString,
            Purged: false);
    }

    // 同一个聚合根在一个命令里发布了多次快照，版本号相同，只留最后一次
    private static IEnumerable<PendingMessage> LatestSnapshots(IReadOnlyList<PendingMessage> pending)
    {
        var latest = new Dictionary<(string, int, Guid), int>();
        for (var i = 0; i < pending.Count; i++)
        {
            if (pending[i].Message is ISnapshotEvent snapshot)
            {
                latest[(pending[i].Type.Name, pending[i].Type.Version, snapshot.SourceId)] = i;
            }
        }

        return pending.Where((message, i) =>
            message.Message is not ISnapshotEvent snapshot || latest[(message.Type.Name, message.Type.Version, snapshot.SourceId)] == i);
    }

    private static Dictionary<Guid, long> AggregateVersions(ModuleDbContext context)
    {
        var versions = new Dictionary<Guid, long>();
        foreach (var entry in context.ChangeTracker.Entries<IAggregateRoot>())
        {
            if (entry.Property(nameof(Entity<>.Id)).CurrentValue is IStronglyTypedId id)
            {
                versions[id.Value] = entry.Entity.ConcurrencyVersion;
            }
        }

        return versions;
    }

    private static long VersionOf(Dictionary<Guid, long> versions, IntegrationMessageType type, Guid sourceId) =>
        versions.TryGetValue(sourceId, out var version)
            ? version
            : throw new InvalidOperationException(
                $"{type.Name} 的 SourceId {sourceId} 在这次保存里找不到对应的聚合根。快照事件要和聚合根的修改在同一个命令里发布。");
}

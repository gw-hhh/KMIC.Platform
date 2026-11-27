using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 须在业务数据保存后、事务提交前调用，将待发消息写入所属模块的 Outbox
internal sealed class OutboxWriter(
    IntegrationEventPublisher publisher,
    IExecutionContext execution,
    CurrentInboxMessage inbox,
    MessagingAdmission admission,
    FrameworkMetrics metrics,
    TimeProvider time,
    IOptions<MessagingOptions> options,
    ILogger<OutboxWriter> logger)
{
    public void Discard() => publisher.Discard();

    public Task WriteAsync(ModuleDbContext context, string module, bool exemptFromSoftLine, CancellationToken ct) =>
        WriteAsync(context, module, publisher.TakePending(), exemptFromSoftLine ? AdmissionMode.ExemptFromSoftLine : AdmissionMode.Enforce, ct);

    // 供读取审计等非命令场景直接写入消息
    public Task WriteAsync(ModuleDbContext context, string module, IReadOnlyList<PendingMessage> pending, CancellationToken ct) =>
        WriteAsync(context, module, pending, AdmissionMode.Enforce, ct);

    // 触发容量保护且未配置 SkipWhenSuspended 时抛出 MessagingSuspendedException
    public async Task WriteAsync(ModuleDbContext context, string module, IReadOnlyList<PendingMessage> pending, AdmissionMode mode, CancellationToken ct)
    {
        if (pending.Count == 0)
        {
            return;
        }

        if (!IsAdmitted(module, pending.Count, mode))
        {
            return;
        }

        var limits = options.Value;
        var now = time.GetUtcNow();
        var headers = JsonSerializer.Serialize(Headers(module, execution.User, inbox.MessageId), KmicJsonOptions.Stored);
        var versions = AggregateVersions(context);
        var total = 0;
        foreach (var message in LatestSnapshots(pending))
        {
            var payload = JsonSerializer.Serialize(message.Message, message.Type.ClrType, KmicJsonOptions.Stored);
            var size = Encoding.UTF8.GetByteCount(payload);
            if (size > limits.MaxPayloadBytes)
            {
                throw new InvalidOperationException(
                    $"{message.Type.Name} 的消息体大小为 {size} 字节，超过上限 {limits.MaxPayloadBytes} 字节。大体积内容请转为文件存储，仅在消息中传递文件 ID。");
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
                $"当前命令待写入 Outbox 的总数据量为 {total} 字节，超过单事务上限 {limits.MaxOutboxBytesPerTransaction} 字节。大批量消息请拆分为后台作业分批发布。");
        }

        await context.SaveChangesAsync(ct);

        // 通知于事务提交后送达，确保投递程序唤醒时能够读取到这批消息
        await context.Database.ExecuteSqlAsync($"SELECT pg_notify({MessagingChannels.Outbox}, {module})", ct);
    }

    // 返回 false 表示跳过写入（仅限 SkipWhenSuspended 模式）；其余拒绝情况均抛出异常以回滚业务事务
    private bool IsAdmitted(string module, int count, AdmissionMode mode)
    {
        if (admission.IsAllowed(execution.Origin, mode != AdmissionMode.Enforce))
        {
            return true;
        }

        metrics.MessagingAdmissionRejected(admission.State == AdmissionState.Hard ? "hard" : "soft", execution.Origin.ToString(), mode == AdmissionMode.SkipWhenSuspended);
        if (mode == AdmissionMode.SkipWhenSuspended)
        {
            MessagingLog.AdmissionSkippedWrite(logger, module, count);
            return false;
        }

        throw new MessagingSuspendedException($"数据库空间不足，已暂停消息写入（{admission.State}），{module} 模块本次写入 {count} 条消息的请求已被拒绝。");
    }

    private static MessageHeaders Headers(string module, ICurrentUser user, Guid? causationId)
    {
        var activity = Activity.Current;
        return new MessageHeaders(
            module,
            (user.IsSystem ? user.OnBehalfOf : user.UserId)?.Value,
            user.ActualUserId?.Value,
            causationId,
            activity?.Id,
            activity?.TraceStateString,
            Purged: false);
    }

    // 同一聚合根在单次命令中多次发布同版本快照事件时，仅保留最后一次
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
                $"{type.Name} 的 SourceId（{sourceId}）在本次保存中未匹配到对应聚合根。快照事件须与聚合根变更在同一命令中发布。");
}

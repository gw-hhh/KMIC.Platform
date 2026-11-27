using System.Diagnostics;
using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Auditing;

// 保存前采集变更并生成审计事件，随业务数据一并写入 Outbox
internal sealed class AuditRecorder(
    IExecutionContext execution,
    IntegrationEventPublisher publisher,
    TimeProvider time,
    IOptions<AuditOptions> options) : IAuditContext
{
    private readonly List<AuditTarget> _targets = [];
    private readonly Dictionary<string, string> _details = new(StringComparer.Ordinal);

    public void AddTarget(string resourceType, Guid resourceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceType);
        _targets.Add(new AuditTarget(resourceType, resourceId));
    }

    public void Add(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        _details[key] = AuditValues.Truncate(value, options.Value.MaxValueLength)!;
    }

    // 须在 SaveChanges 前调用，未保存前的变更跟踪状态才完整
    public void Publish(ModuleDbContext context, UseCase useCase)
    {
        var (changes, targets) = AuditChanges.Collect(context, options.Value.MaxValueLength);
        publisher.Add(Build(useCase, changes, targets));
        Clear();
    }

    public AuditRecorded BuildForRead(UseCase useCase)
    {
        var audit = Build(useCase, [], []);
        Clear();
        return audit;
    }

    public void Discard() => Clear();

    // 以作业名为动作，补充信息由调用方通过 Add 添加
    public AuditRecorded BuildForJob(string module, string jobName)
    {
        var audit = Build(module, jobName, [], []);
        Clear();
        return audit;
    }

    private AuditRecorded Build(UseCase useCase, IReadOnlyList<EntityChange> changes, IReadOnlyList<AuditTarget> targets) =>
        Build(useCase.Module, useCase.AuditAction ?? useCase.Module, changes, targets);

    private AuditRecorded Build(string module, string action, IReadOnlyList<EntityChange> changes, IReadOnlyList<AuditTarget> targets)
    {
        var user = execution.User;
        return new AuditRecorded
        {
            AuditId = Guid.CreateVersion7(),
            OccurredAt = time.GetUtcNow(),
            Module = module,
            Action = action,
            Origin = execution.Origin.ToString(),
            ActorUserId = user.UserId?.Value,
            ActualUserId = user.ActualUserId?.Value,
            RoleId = user.RoleId?.Value,
            SessionId = user.SessionId?.Value,
            OnBehalfOfUserId = user.OnBehalfOf?.Value,
            TraceId = Activity.Current?.TraceId.ToString() ?? new string('0', 32),
            Targets = [.. targets.Concat(_targets).Distinct()],
            Changes = changes,
            Details = new Dictionary<string, string>(_details, StringComparer.Ordinal),
        };
    }

    private void Clear()
    {
        _targets.Clear();
        _details.Clear();
    }
}

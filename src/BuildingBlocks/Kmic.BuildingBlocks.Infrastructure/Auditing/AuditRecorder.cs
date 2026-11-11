using System.Diagnostics;
using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Auditing;

/// <summary>
/// 一次命令的审计：收集命令补充的信息，保存前生成审计事件，和业务数据一起写进 Outbox
/// </summary>
/// <remarks>
/// 不自动记录命令参数，里面可能有敏感数据，要记什么由命令自己通过 IAuditContext 写
/// </remarks>
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

    /// <summary>
    /// 采集这次保存的变更，生成审计事件交给发布器
    /// </summary>
    /// <remarks>
    /// 在保存之前调用。命令成功但没有任何数据变更时，也记一条没有变更的事件
    /// </remarks>
    public void Publish(ModuleDbContext context, UseCase useCase)
    {
        var (changes, targets) = AuditChanges.Collect(context, options.Value.MaxValueLength);
        publisher.Add(Build(useCase, changes, targets));
        Clear();
    }

    /// <summary>
    /// 敏感查询成功之后的审计事件：没有数据变更，查询条件等由查询用 IAuditContext 补充
    /// </summary>
    public AuditRecorded BuildForRead(UseCase useCase)
    {
        var audit = Build(useCase, [], []);
        Clear();
        return audit;
    }

    /// <summary>
    /// 命令回滚时丢弃已收集的补充信息
    /// </summary>
    public void Discard() => Clear();

    /// <summary>
    /// 作业结束时的审计事件，作业名作为动作，说明由调用方通过 Add 补充
    /// </summary>
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

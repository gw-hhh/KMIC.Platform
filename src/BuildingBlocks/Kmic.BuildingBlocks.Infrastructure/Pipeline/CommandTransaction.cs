using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Caching;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 命令事务
/// </summary>
/// <remarks>
/// 登记提交前要和业务数据一起写入的记录（如幂等完成标记），以及提交成功后才执行的动作
/// </remarks>
internal sealed class CommandTransaction(CacheInvalidator cache) : IAfterCommit
{
    private readonly List<Func<ModuleDbContext, object?, CancellationToken, Task<Error?>>> _beforeCommit = [];
    private readonly List<Func<CancellationToken, ValueTask>> _afterCommit = [];
    private readonly HashSet<string> _tags = new(StringComparer.Ordinal);
    private bool _invalidationQueued;

    /// <summary>
    /// 当前命令所在的模块，只有这个模块的写入上下文会被保存
    /// </summary>
    public string? Module { get; set; }

    public void Enqueue(Func<CancellationToken, ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _afterCommit.Add(action);
    }

    // 提交成功之后才失效：提交前清掉的话，并发的请求会用旧数据把缓存重新填上。同一事务登记的标签合并去重
    public void InvalidateTags(params IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);
        _tags.UnionWith(tags);
        if (_invalidationQueued)
        {
            return;
        }

        _invalidationQueued = true;
        _afterCommit.Add(async ct =>
        {
            var pending = _tags.ToArray();
            _tags.Clear();
            await cache.InvalidateAsync(pending, ct);
        });
    }

    /// <summary>
    /// 登记提交前的动作
    /// </summary>
    /// <remarks>
    /// 在业务事务里执行，返回错误时整个命令回滚
    /// </remarks>
    public void BeforeCommit(Func<ModuleDbContext, object?, CancellationToken, Task<Error?>> hook)
    {
        ArgumentNullException.ThrowIfNull(hook);
        _beforeCommit.Add(hook);
    }

    public async Task<Error?> RunBeforeCommitAsync(ModuleDbContext context, object? responseValue, CancellationToken ct)
    {
        foreach (var hook in _beforeCommit)
        {
            if (await hook(context, responseValue, ct) is { } error)
            {
                return error;
            }
        }

        return null;
    }

    public IReadOnlyList<Func<CancellationToken, ValueTask>> TakeAfterCommit()
    {
        var actions = _afterCommit.ToArray();
        _afterCommit.Clear();
        _invalidationQueued = false;
        return actions;
    }

    // 回滚时丢弃，失败的命令不该留下提交后的动作
    public void Discard()
    {
        _beforeCommit.Clear();
        _afterCommit.Clear();
        _tags.Clear();
        _invalidationQueued = false;
    }
}

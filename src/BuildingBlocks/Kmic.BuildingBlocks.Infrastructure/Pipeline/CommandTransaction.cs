using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Caching;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

internal sealed class CommandTransaction(CacheInvalidator cache) : IAfterCommit
{
    private readonly List<Func<ModuleDbContext, object?, CancellationToken, Task<Error?>>> _beforeCommit = [];
    private readonly List<Func<CancellationToken, ValueTask>> _afterCommit = [];
    private readonly HashSet<string> _tags = new(StringComparer.Ordinal);
    private bool _invalidationQueued;

    // 仅保存当前命令所属模块的写入上下文
    public string? Module { get; set; }

    public void Enqueue(Func<CancellationToken, ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _afterCommit.Add(action);
    }

    // 仅在提交成功后失效缓存：若在提交前清除，并发请求可能将旧数据重新回填缓存；同一事务内登记的标签会合并去重
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

    // 在业务事务内执行，返回错误时触发整个命令回滚
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

    // 事务回滚时丢弃，避免失败的命令触发提交后动作
    public void Discard()
    {
        _beforeCommit.Clear();
        _afterCommit.Clear();
        _tags.Clear();
        _invalidationQueued = false;
    }
}

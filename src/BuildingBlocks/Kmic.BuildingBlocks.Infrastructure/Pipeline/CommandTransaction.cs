using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 命令事务
/// </summary>
/// <remarks>
/// 登记提交前要和业务数据一起写入的记录（如幂等完成标记），以及提交成功后才执行的动作
/// </remarks>
internal sealed class CommandTransaction : IAfterCommit
{
    private readonly List<Func<ModuleDbContext, object?, CancellationToken, Task<Error?>>> _beforeCommit = [];
    private readonly List<Func<CancellationToken, ValueTask>> _afterCommit = [];

    public void Enqueue(Func<CancellationToken, ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _afterCommit.Add(action);
    }

    // 暂不支持：缓存失效要广播到其他实例，等缓存功能一起实现
    public void InvalidateTags(params IEnumerable<string> tags) =>
        throw new NotSupportedException("暂不支持缓存失效。");

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
        return actions;
    }

    // 回滚时丢弃，失败的命令不该留下提交后的动作
    public void Discard()
    {
        _beforeCommit.Clear();
        _afterCommit.Clear();
    }
}

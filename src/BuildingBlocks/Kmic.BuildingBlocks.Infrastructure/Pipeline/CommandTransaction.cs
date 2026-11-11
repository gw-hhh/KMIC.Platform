using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;

namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 当前命令事务的挂接点：提交前与业务数据一起写入的框架记录（幂等完成标记等），以及提交成功后才执行的动作
/// </summary>
/// <remarks>
/// 每个作用域一个，事务行为在提交前后依次取出执行
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

    // 缓存失效经 PG NOTIFY 广播到其他实例，随配置与缓存一起实现（S2-09）
    public void InvalidateTags(params IEnumerable<string> tags) =>
        throw new NotSupportedException("缓存失效随配置与缓存（S2-09）实现。");

    /// <summary>
    /// 登记提交前的动作：在业务事务中执行，参数是上下文与成功响应的值；返回错误则整个命令回滚
    /// </summary>
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

    // 回滚时丢弃：失败的命令不该留下任何提交后的动作
    public void Discard()
    {
        _beforeCommit.Clear();
        _afterCommit.Clear();
    }
}

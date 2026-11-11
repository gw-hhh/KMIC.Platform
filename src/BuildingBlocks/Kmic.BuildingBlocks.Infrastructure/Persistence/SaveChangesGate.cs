namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 保存守卫：只有事务行为打开它之后才能保存，处理器自己调用 SaveChangesAsync 即抛异常
/// </summary>
/// <remarks>
/// 每个作用域一个；处理器只做编排，保存、开事务、写 Outbox 都由事务行为完成（05-02 第 2.2 节）
/// </remarks>
public sealed class SaveChangesGate
{
    private int _openCount;

    internal IDisposable Open()
    {
        _openCount++;
        return new Closer(this);
    }

    internal void EnsureOpen()
    {
        if (_openCount == 0)
        {
            throw new InvalidOperationException("只有事务行为能保存：处理器不要调用 SaveChangesAsync，返回结果后由管道统一提交。");
        }
    }

    private sealed class Closer(SaveChangesGate gate) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                gate._openCount--;
            }
        }
    }
}

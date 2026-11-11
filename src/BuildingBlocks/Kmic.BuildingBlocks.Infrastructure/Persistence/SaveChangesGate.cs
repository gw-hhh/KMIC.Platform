namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 保存入口控制
/// </summary>
/// <remarks>
/// 只有命令事务打开它之后才能保存，处理器自己调用 SaveChangesAsync 会抛异常
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
            throw new InvalidOperationException("处理器不能自己保存，返回结果后由命令事务统一保存。");
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

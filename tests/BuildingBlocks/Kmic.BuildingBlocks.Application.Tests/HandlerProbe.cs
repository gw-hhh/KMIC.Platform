using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Application.Execution;

namespace Kmic.BuildingBlocks.Application.Tests;

/// <summary>
/// 记录处理器是否被调用、调用时的执行上下文
/// </summary>
public sealed class HandlerProbe
{
    private readonly ConcurrentQueue<(string Handler, CallOrigin Origin)> _calls = new();

    public IReadOnlyCollection<(string Handler, CallOrigin Origin)> Calls => _calls;

    public void Record(string handler, CallOrigin origin) => _calls.Enqueue((handler, origin));
}

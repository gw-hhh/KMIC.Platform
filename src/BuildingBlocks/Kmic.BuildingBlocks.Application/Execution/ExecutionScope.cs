using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>执行上下文；Initialize 仅限框架入口调用。</summary>
public sealed class ExecutionScope : IExecutionContext
{
    // 按异步调用链隔离，同一作用域内并行的其他调用互不影响
    private static readonly AsyncLocal<int> _moduleCallDepth = new();
    private static readonly AsyncLocal<int> _commandDepth = new();

    private CallOrigin? _origin;

    public CallOrigin Origin => _moduleCallDepth.Value > 0 ? CallOrigin.ModuleCall : EntryOrigin;

    /// <summary>入口调用来源；跨模块 Contracts 调用期间保持不变（此时 Origin 为 ModuleCall）。</summary>
    public CallOrigin EntryOrigin =>
        _origin ?? throw new InvalidOperationException("执行上下文尚未初始化，入口应先调用 ExecutionScope.Initialize。");

    public ICurrentUser User { get; private set; } = CurrentUser.Anonymous;

    public IAccessContext? Access { get; private set; }

    public bool InCommand => _commandDepth.Value > 0;

    /// <summary>当前异步调用链是否处于命令执行中。</summary>
    public static bool CurrentlyInCommand => _commandDepth.Value > 0;

    /// <summary>初始化请求来源与身份信息，每个作用域仅允许调用一次。</summary>
    public void Initialize(CallOrigin origin, ICurrentUser user, IAccessContext? access = null)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (_origin is not null)
        {
            throw new InvalidOperationException("执行上下文已经初始化，同一作用域不能更换来源或身份。");
        }

        _origin = origin;
        User = user;
        Access = access;
    }

    internal static IDisposable EnterModuleCall() => new DepthMarker(_moduleCallDepth);

    internal IDisposable EnterCommand()
    {
        if (InCommand)
        {
            throw new InvalidOperationException("命令中不可嵌套发送命令；复用逻辑请抽至领域服务，跨模块写操作请发布集成事件。");
        }

        return new DepthMarker(_commandDepth);
    }

    private sealed class DepthMarker : IDisposable
    {
        private readonly AsyncLocal<int> _depth;

        public DepthMarker(AsyncLocal<int> depth)
        {
            _depth = depth;
            _depth.Value++;
        }

        public void Dispose() => _depth.Value--;
    }
}

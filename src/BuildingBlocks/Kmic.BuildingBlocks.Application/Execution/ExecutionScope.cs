using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 执行上下文
/// </summary>
/// <remarks>
/// 模块代码只注入 IExecutionContext，Initialize 只给框架入口调用
/// </remarks>
public sealed class ExecutionScope : IExecutionContext
{
    // 按异步调用链生效，同一作用域里并行的其他调用不受影响
    private static readonly AsyncLocal<int> _moduleCallDepth = new();
    private static readonly AsyncLocal<int> _commandDepth = new();

    private CallOrigin? _origin;

    /// <inheritdoc />
    public CallOrigin Origin => _moduleCallDepth.Value > 0 ? CallOrigin.ModuleCall : EntryOrigin;

    /// <summary>
    /// 入口的来源
    /// </summary>
    /// <remarks>
    /// Contracts 调用期间 Origin 会变成 ModuleCall，这里仍是入口的来源，用来选数据源
    /// </remarks>
    public CallOrigin EntryOrigin =>
        _origin ?? throw new InvalidOperationException("执行上下文尚未初始化，入口应先调用 ExecutionScope.Initialize。");

    /// <inheritdoc />
    public ICurrentUser User { get; private set; } = CurrentUser.Anonymous;

    /// <inheritdoc />
    public IAccessContext? Access { get; private set; }

    /// <inheritdoc />
    public bool InCommand => _commandDepth.Value > 0;

    /// <summary>
    /// 设置来源和身份，每个作用域只能设置一次
    /// </summary>
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
            throw new InvalidOperationException("命令里不能再发命令。要复用的逻辑放进领域服务，要别的模块改数据就发集成事件。");
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

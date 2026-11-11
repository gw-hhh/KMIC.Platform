using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 执行上下文
/// </summary>
public interface IExecutionContext
{
    /// <summary>
    /// 请求来源
    /// </summary>
    CallOrigin Origin { get; }

    /// <summary>
    /// 当前身份
    /// </summary>
    ICurrentUser User { get; }

    /// <summary>
    /// 访问上下文，系统身份和匿名请求时为空
    /// </summary>
    IAccessContext? Access { get; }

    /// <summary>
    /// 是否正在执行命令
    /// </summary>
    bool InCommand { get; }
}

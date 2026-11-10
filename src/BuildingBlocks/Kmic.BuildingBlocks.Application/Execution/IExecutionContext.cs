using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 当前作用域的执行上下文：来源、身份与访问上下文
/// </summary>
public interface IExecutionContext
{
    /// <summary>
    /// 调用来源；Contracts 调用期间为 ModuleCall
    /// </summary>
    CallOrigin Origin { get; }

    /// <summary>
    /// 执行身份
    /// </summary>
    ICurrentUser User { get; }

    /// <summary>
    /// 用户身份时的访问上下文（权限点、数据范围输入）；系统身份与匿名请求为空
    /// </summary>
    IAccessContext? Access { get; }

    /// <summary>
    /// 是否正在执行命令
    /// </summary>
    bool InCommand { get; }
}

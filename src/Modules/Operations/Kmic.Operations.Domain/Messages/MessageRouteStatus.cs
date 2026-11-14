using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>
/// 订阅路由的状态
/// </summary>
/// <remarks>
/// 编码和框架路由表里的状态一一对应，领域层不引用基础设施，所以单独定义
/// </remarks>
public sealed class MessageRouteStatus : Enumeration<MessageRouteStatus>
{
    /// <summary>
    /// 生效：照常投递
    /// </summary>
    public static readonly MessageRouteStatus Active = new("Active", "生效");

    /// <summary>
    /// 待终结：订阅已从代码里去掉，消息照常投递并积压，等运维确认
    /// </summary>
    public static readonly MessageRouteStatus Retiring = new("Retiring", "待终结");

    /// <summary>
    /// 已终结：不再投递
    /// </summary>
    public static readonly MessageRouteStatus Finalized = new("Finalized", "已终结");

    private MessageRouteStatus(string code, string name)
        : base(code, name)
    {
    }
}

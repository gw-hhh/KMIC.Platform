using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>
/// 订阅路由的状态
/// </summary>
/// <remarks>
/// 编码与框架路由表的状态一一对应
/// </remarks>
public sealed class MessageRouteStatus : Enumeration<MessageRouteStatus>
{
    /// <summary>
    /// 生效：照常投递
    /// </summary>
    public static readonly MessageRouteStatus Active = new("Active", "生效");

    /// <summary>
    /// 继续投递，等待运维确认终结
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

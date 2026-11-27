using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Operations.Domain.Messages;

/// <summary>订阅路由状态（编码与框架路由表状态一一对应）。</summary>
public sealed class MessageRouteStatus : Enumeration<MessageRouteStatus>
{
    public static readonly MessageRouteStatus Active = new("Active", "生效");

    /// <summary>继续投递，等待运维确认终结。</summary>
    public static readonly MessageRouteStatus Retiring = new("Retiring", "待终结");

    /// <summary>已终结，不再投递。</summary>
    public static readonly MessageRouteStatus Finalized = new("Finalized", "已终结");

    private MessageRouteStatus(string code, string name)
        : base(code, name)
    {
    }
}

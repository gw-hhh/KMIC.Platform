namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 消息的稳定标识：Outbox / Inbox 按名称与版本路由和反序列化，与类名、命名空间无关
/// </summary>
/// <remarks>
/// 名称为“模块.类名”，以本模块名开头且全系统唯一（启动校验）；框架消息以 Framework. 开头。
/// 破坏性变化时新建 Version = 2 的消息类，名称不变
/// </remarks>
/// <param name="name">稳定名称，如 Customer.CustomerChanged</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IntegrationMessageAttribute(string name) : Attribute
{
    /// <summary>
    /// 稳定名称，如 Customer.CustomerChanged
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 契约版本，从 1 开始；只有破坏性变化才递增
    /// </summary>
    public int Version { get; init; } = 1;
}

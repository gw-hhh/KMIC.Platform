namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 消息名称和版本
/// </summary>
/// <remarks>
/// 名称格式为“模块.类名”，全系统唯一。有不兼容的改动时，新建一个 Version = 2 的类，名称不变
/// </remarks>
/// <param name="name">消息名称，如 Customer.CustomerChanged</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IntegrationMessageAttribute(string name) : Attribute
{
    /// <summary>
    /// 消息名称
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 版本号，从 1 开始
    /// </summary>
    public int Version { get; init; } = 1;
}

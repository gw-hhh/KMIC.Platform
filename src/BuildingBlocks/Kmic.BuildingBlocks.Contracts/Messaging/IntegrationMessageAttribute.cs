namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 消息名称和版本
/// </summary>
/// <remarks>
/// 不兼容的改动新建一个 Version = 2 的类，名称不变
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IntegrationMessageAttribute(string name) : Attribute
{
    /// <summary>
    /// 单个含密消息的凭据有效期上限，秒
    /// </summary>
    public const int MaxSecretLifetimeSeconds = 600;

    /// <summary>
    /// 消息名称
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 版本号，从 1 开始
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// 载荷里带一次性短时凭据，如短信验证码
    /// </summary>
    /// <remarks>
    /// 要同时写 <see cref="SecretLifetimeSeconds" />，只有请求型消息能声明
    /// </remarks>
    public bool CarriesSecret { get; init; }

    /// <summary>
    /// 含密消息里凭据的有效期，秒
    /// </summary>
    public int SecretLifetimeSeconds { get; init; }
}

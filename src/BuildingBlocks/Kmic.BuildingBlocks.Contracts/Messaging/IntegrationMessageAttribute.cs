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
    /// 载荷里带有一次性短时凭据，如短信验证码
    /// </summary>
    /// <remarks>
    /// 消息载荷不能放密码和令牌，这是唯一的例外：凭据只存哈希、用后即废，并且必须同时写明有效期
    /// （<see cref="SecretLifetimeSeconds" />，不超过 <see cref="MaxSecretLifetimeSeconds" />）。
    /// 只有请求型消息可以声明，载荷不进日志
    /// </remarks>
    public bool CarriesSecret { get; init; }

    /// <summary>
    /// 含密消息里凭据的有效期，秒
    /// </summary>
    public int SecretLifetimeSeconds { get; init; }
}

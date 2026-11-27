namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>声明集成消息的名称与版本；发生不兼容变更时须新建 Version = 2 的消息类，并保持名称不变。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IntegrationMessageAttribute(string name) : Attribute
{
    /// <summary>敏感消息凭据的有效期上限（秒）。</summary>
    public const int MaxSecretLifetimeSeconds = 600;

    /// <summary>消息名称。</summary>
    public string Name { get; } = name;

    /// <summary>版本号，从 1 开始。</summary>
    public int Version { get; init; } = 1;

    /// <summary>是否含短时凭据（如短信验证码）；启用时须同步指定 <see cref="SecretLifetimeSeconds" />，且仅限请求型消息声明。</summary>
    public bool CarriesSecret { get; init; }

    /// <summary>敏感凭据有效期，单位秒。</summary>
    public int SecretLifetimeSeconds { get; init; }
}

namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>声明通知模板，标注于模板数据模型之上。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotificationTemplateAttribute(string key, string displayName) : Attribute
{
    /// <summary>默认发件账号配置名，对应部署配置 Notifications:Email:Senders 下的节点。</summary>
    public const string DefaultSenderProfile = "Default";

    /// <summary>模板键。</summary>
    public string Key { get; } = key;

    /// <summary>显示名称。</summary>
    public string DisplayName { get; } = displayName;

    /// <summary>通知发送渠道。</summary>
    public NotificationChannels Channels { get; init; } = NotificationChannels.None;

    /// <summary>发件账号名，仅对邮件渠道生效。</summary>
    public string SenderProfile { get; init; } = DefaultSenderProfile;

    /// <summary>是否含短时凭据（如验证码），仅限通过 <c>SendSecretNotification</c> 发送。</summary>
    public bool CarriesSecret { get; init; }
}

namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>
/// 声明一种通知，标在模板的数据模型上
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotificationTemplateAttribute(string key, string displayName) : Attribute
{
    /// <summary>
    /// 默认的发件账号名，对应部署配置 Notifications:Email:Senders 下的一项
    /// </summary>
    public const string DefaultSenderProfile = "Default";

    /// <summary>
    /// 模板键
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 这种通知发往哪些渠道
    /// </summary>
    public NotificationChannels Channels { get; init; } = NotificationChannels.None;

    /// <summary>
    /// 发件账号名，只对邮件渠道有意义
    /// </summary>
    public string SenderProfile { get; init; } = DefaultSenderProfile;

    /// <summary>
    /// 内容含一次性短时凭据（验证码）
    /// </summary>
    /// <remarks>
    /// 只能用 <c>SendSecretNotification</c> 发送
    /// </remarks>
    public bool CarriesSecret { get; init; }
}

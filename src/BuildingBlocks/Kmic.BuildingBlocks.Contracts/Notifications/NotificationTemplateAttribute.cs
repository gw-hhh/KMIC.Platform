namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>
/// 声明一种通知：标在模板的数据模型上
/// </summary>
/// <remarks>
/// 模板由代码声明，管理员只能修改内容；模板键以声明它的模块名开头，全系统唯一。
/// 数据模型要实现 <see cref="INotificationModel{TSelf}" />
/// </remarks>
/// <param name="key">模板键，如 Identity.SmsCode</param>
/// <param name="displayName">管理页里显示的名称</param>
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
    /// 含密模板只能用 <c>SendSecretNotification</c> 发送：投递完成后正文清空，渲染结果不写日志，不带附件
    /// </remarks>
    public bool CarriesSecret { get; init; }
}

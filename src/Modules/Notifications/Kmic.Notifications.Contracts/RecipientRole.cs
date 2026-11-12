namespace Kmic.Notifications.Contracts;

/// <summary>
/// 邮件收件人的角色
/// </summary>
public enum RecipientRole
{
    /// <summary>
    /// 收件人
    /// </summary>
    To,

    /// <summary>
    /// 抄送
    /// </summary>
    Cc,

    /// <summary>
    /// 密送
    /// </summary>
    Bcc,
}

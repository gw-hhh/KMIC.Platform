namespace Kmic.Notifications.Contracts;

/// <summary>
/// 通知收件人
/// </summary>
/// <remarks>
/// 系统用户优先使用用户 ID，由 Notifications 向 Identity 查询联系方式；
/// 仅外部收件人直接指定邮箱或手机号，地址由业务模块提供
/// </remarks>
/// <param name="UserId">用户 ID</param>
/// <param name="Email">系统外对象的邮箱</param>
/// <param name="Phone">系统外对象的手机号</param>
/// <param name="Role">邮件收件角色，仅邮件渠道使用</param>
public sealed record Recipient(Guid? UserId, string? Email = null, string? Phone = null, RecipientRole Role = RecipientRole.To)
{
    /// <summary>
    /// 系统里的用户
    /// </summary>
    public static Recipient OfUser(Guid userId, RecipientRole role = RecipientRole.To) => new(userId, Role: role);

    /// <summary>
    /// 系统外对象的邮箱
    /// </summary>
    public static Recipient OfEmail(string email, RecipientRole role = RecipientRole.To) => new(null, Email: email, Role: role);

    /// <summary>
    /// 系统外对象的手机号
    /// </summary>
    public static Recipient OfPhone(string phone) => new(null, Phone: phone);
}

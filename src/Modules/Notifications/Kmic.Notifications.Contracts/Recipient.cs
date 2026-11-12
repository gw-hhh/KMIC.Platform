namespace Kmic.Notifications.Contracts;

/// <summary>
/// 通知的收件人
/// </summary>
/// <remarks>
/// 优先用用户 ID，联系方式由 Notifications 在发送时向 Identity 查询；直接给邮箱或手机号只用于系统外的对象，
/// 如客户联系人，地址由业务模块从自己的数据里取
/// </remarks>
/// <param name="UserId">用户 ID</param>
/// <param name="Email">系统外对象的邮箱</param>
/// <param name="Phone">系统外对象的手机号</param>
/// <param name="Role">邮件里的角色，只对邮件有意义</param>
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

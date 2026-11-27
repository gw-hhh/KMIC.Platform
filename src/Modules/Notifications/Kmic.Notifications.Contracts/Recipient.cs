namespace Kmic.Notifications.Contracts;

/// <summary>通知收件人（系统内用户仅需传 UserId，联系方式由 Identity 模块查询）</summary>
/// <param name="UserId">用户 ID</param>
/// <param name="Email">外部收件人邮箱</param>
/// <param name="Phone">外部收件人手机号</param>
/// <param name="Role">邮件收件方式</param>
public sealed record Recipient(Guid? UserId, string? Email = null, string? Phone = null, RecipientRole Role = RecipientRole.To)
{
    /// <summary>系统内用户</summary>
    public static Recipient OfUser(Guid userId, RecipientRole role = RecipientRole.To) => new(userId, Role: role);

    /// <summary>外部邮箱收件人</summary>
    public static Recipient OfEmail(string email, RecipientRole role = RecipientRole.To) => new(null, Email: email, Role: role);

    /// <summary>外部手机号收件人</summary>
    public static Recipient OfPhone(string phone) => new(null, Phone: phone);
}

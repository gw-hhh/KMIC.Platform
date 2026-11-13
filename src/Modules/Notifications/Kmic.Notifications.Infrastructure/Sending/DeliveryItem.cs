namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 投递队列里领取到的一条
/// </summary>
/// <param name="Id">投递 ID</param>
/// <param name="Channel">渠道编码</param>
/// <param name="RecipientUserId">收件人，系统里的用户；直接给了地址的为空</param>
/// <param name="Address">直接给的地址；用户收件人为空，发送时向 Identity 取最新的</param>
/// <param name="Role">邮件里的角色编码</param>
/// <param name="Subject">主题</param>
/// <param name="Body">渲染好的正文</param>
/// <param name="SmsParameters">短信模板的参数，JSON</param>
/// <param name="TemplateKey">模板键</param>
/// <param name="Attempts">包括这一次在内已经尝试几次</param>
/// <param name="CreatedAt">投递创建的时间</param>
internal sealed record DeliveryItem(
    Guid Id,
    string Channel,
    Guid? RecipientUserId,
    string? Address,
    string Role,
    string Subject,
    string? Body,
    string? SmsParameters,
    string TemplateKey,
    int Attempts,
    DateTimeOffset CreatedAt);

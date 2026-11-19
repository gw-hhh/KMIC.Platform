namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 投递队列中已领取的记录
/// </summary>
/// <param name="Id">投递 ID</param>
/// <param name="Channel">渠道编码</param>
/// <param name="RecipientUserId">系统内收件人 ID，外部收件人为空</param>
/// <param name="Address">外部收件人地址，系统内用户为空</param>
/// <param name="Role">邮件收件方式编码</param>
/// <param name="Subject">主题</param>
/// <param name="Body">渲染好的正文</param>
/// <param name="SmsParameters">短信模板的参数</param>
/// <param name="AttachmentFileIds">邮件附件的文件 ID</param>
/// <param name="TemplateKey">模板键</param>
/// <param name="Attempts">领取次数</param>
/// <param name="CreatedAt">投递创建时间</param>
internal sealed record DeliveryItem(
    Guid Id,
    string Channel,
    Guid? RecipientUserId,
    string? Address,
    string Role,
    string Subject,
    string? Body,
    string? SmsParameters,
    string? AttachmentFileIds,
    string TemplateKey,
    int Attempts,
    DateTimeOffset CreatedAt);

using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Notifications.Application.Channels;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 发送邮件时读取附件：投递记录里只存文件 ID，内容到发送时才向 Files 取
/// </summary>
/// <remarks>
/// 附件永远读不到（文件不存在、已被彻底删除）、总大小超过上限，这封邮件都发不出去，算永久失败；
/// 文件还没登记完、对象正在恢复这类稍后能读到的，算暂时失败，退避之后重试。
/// 打开的流在成功时交给调用方释放，失败时这里已经全部释放
/// </remarks>
internal sealed class AttachmentLoader(IFileContents files)
{
    public async Task<AttachmentLoad> LoadAsync(IReadOnlyList<Guid> fileIds, long maxBytes, CancellationToken ct)
    {
        var opened = new List<EmailAttachment>(fileIds.Count);
        long total = 0;
        try
        {
            foreach (var id in fileIds)
            {
                var content = await files.OpenReadAsync(id, ct);
                if (content.IsFailure)
                {
                    await ReleaseAsync(opened);
                    return AttachmentLoad.Failed(FailureOf(content.Error));
                }

                opened.Add(new EmailAttachment(content.Value.Name, content.Value.ContentType, content.Value.Content));
                total += content.Value.Size;
                if (total > maxBytes)
                {
                    await ReleaseAsync(opened);
                    return AttachmentLoad.Failed(SendResult.Permanent($"附件加起来超过了 {maxBytes / (1024 * 1024)} MB"));
                }
            }

            return AttachmentLoad.Loaded(opened);
        }
        catch
        {
            await ReleaseAsync(opened);
            throw;
        }
    }

    // 文件没了，等多久都读不到；其余（还没准备好、对象正在恢复）稍后可以再试
    private static SendResult FailureOf(Error error) =>
        error.Type is ErrorType.NotFound or ErrorType.Gone
            ? SendResult.Permanent("附件不存在或已经被删除")
            : SendResult.Transient("附件暂时读不到");

    private static async Task ReleaseAsync(List<EmailAttachment> attachments)
    {
        foreach (var attachment in attachments)
        {
            await attachment.Content.DisposeAsync();
        }

        attachments.Clear();
    }
}

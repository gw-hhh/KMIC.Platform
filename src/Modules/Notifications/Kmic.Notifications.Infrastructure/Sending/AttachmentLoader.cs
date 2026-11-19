using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Notifications.Application.Channels;

namespace Kmic.Notifications.Infrastructure.Sending;

/// <summary>
/// 发送邮件时读取附件
/// </summary>
/// <remarks>
/// 失败时这里负责清理已打开的流
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
                    return AttachmentLoad.Failed(SendResult.Permanent($"附件总大小超过 {maxBytes / (1024 * 1024)} MB"));
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

    // 文件不存在或已删除时停止自动重试；其他读取错误可稍后重试
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

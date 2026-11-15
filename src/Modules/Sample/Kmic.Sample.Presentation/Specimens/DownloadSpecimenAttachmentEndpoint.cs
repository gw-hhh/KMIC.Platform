using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Specimens.Attachments.GetContent;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Presentation.Specimens;

internal sealed class DownloadSpecimenAttachmentEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/specimens/{id}/attachments/{attachmentId}/content", HandleAsync)
            .Sends<GetSpecimenAttachmentContentQuery>()
            .WithSummary("下载示例附件")
            .WithDescription("只有能查看示例的用户才能下载其附件。示例不存在或不在当前用户的数据范围内时返回 404。默认下载文件；PDF 和图片可通过 inline=true 在浏览器中打开。支持 ETag；本地存储支持 Range，S3 存储返回完整文件。成功获取下载信息后记录审计日志。文件尚未就绪返回 409，已彻底删除返回 410。");

    private static async Task<IResult> HandleAsync(
        [Description("示例 ID")] SpecimenId id,
        [Description("附件 ID")] SpecimenAttachmentId attachmentId,
        [Description("在浏览器里直接打开；只有 PDF 和图片有效")] bool? inline,
        IQuerySender sender,
        CancellationToken ct)
    {
        var result = await sender.SendAsync(new GetSpecimenAttachmentContentQuery(id, attachmentId), ct);
        return result.IsSuccess ? FileResults.Stream(result.Value, inline ?? false) : result.ToOk();
    }
}

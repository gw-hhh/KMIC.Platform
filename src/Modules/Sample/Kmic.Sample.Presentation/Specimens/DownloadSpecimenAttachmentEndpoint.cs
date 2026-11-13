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
            .WithSummary("下载示例标本的附件")
            .WithDescription("权限完全继承标本：看不到的标本和不存在的一样，返回 404。默认作为附件下载；PDF 和图片加 inline=true 可以在浏览器里直接打开。支持 Range 和 ETag。每次下载都有审计记录。文件还没准备好返回 409，已被彻底删除返回 410。");

    private static async Task<IResult> HandleAsync(
        [Description("标本 ID")] SpecimenId id,
        [Description("附件 ID")] SpecimenAttachmentId attachmentId,
        [Description("在浏览器里直接打开；只有 PDF 和图片有效")] bool? inline,
        IQuerySender sender,
        CancellationToken ct)
    {
        var result = await sender.SendAsync(new GetSpecimenAttachmentContentQuery(id, attachmentId), ct);
        return result.IsSuccess ? FileResults.Stream(result.Value, inline ?? false) : result.ToOk();
    }
}

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
            .WithDescription("有权查看示例即可下载（记录审计），PDF/图片支持 inline=true 预览，支持 ETag（本地支持 Range）；未就绪返回 409，已彻底删除返回 410，不存在或越权返回 404");

    private static async Task<IResult> HandleAsync(
        [Description("示例 ID")] SpecimenId id,
        [Description("附件 ID")] SpecimenAttachmentId attachmentId,
        [Description("是否在浏览器中直接打开；仅对 PDF 和图片有效")] bool? inline,
        IQuerySender sender,
        CancellationToken ct)
    {
        var result = await sender.SendAsync(new GetSpecimenAttachmentContentQuery(id, attachmentId), ct);
        return result.IsSuccess ? FileResults.Stream(result.Value, inline ?? false) : result.ToOk();
    }
}

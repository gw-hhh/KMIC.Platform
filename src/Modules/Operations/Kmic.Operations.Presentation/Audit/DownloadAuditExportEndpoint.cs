using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Audit.Export;

namespace Kmic.Operations.Presentation.Audit;

internal sealed class DownloadAuditExportEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/audit/exports/{jobId}/file", HandleAsync)
            .Sends<GetAuditExportFileQuery>()
            .WithSummary("下载审计导出的文件")
            .WithDescription("仅提交人或有管理作业权限的用户可下载；无权查看或作业不存在时返回 404。作业未完成返回可重试的 409，失败或取消返回 422。文件未登记或不存在时返回 404。下载会记录读审计。");

    private static async Task<IResult> HandleAsync([Description("导出作业 ID")] Guid jobId, IQuerySender sender, CancellationToken ct)
    {
        var result = await sender.SendAsync(new GetAuditExportFileQuery(jobId), ct);
        return result.IsSuccess ? FileResults.Stream(result.Value, inline: false) : result.ToOk();
    }
}

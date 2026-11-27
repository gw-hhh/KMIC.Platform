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
            .WithSummary("下载审计导出文件")
            .WithDescription("下载导出文件（记录读取审计；下载他人导出还须作业管理权限）；未完成返回可重试的 409，失败或已取消返回 422，不存在或无权返回 404");

    private static async Task<IResult> HandleAsync([Description("导出作业 ID")] Guid jobId, IQuerySender sender, CancellationToken ct)
    {
        var result = await sender.SendAsync(new GetAuditExportFileQuery(jobId), ct);
        return result.IsSuccess ? FileResults.Stream(result.Value, inline: false) : result.ToOk();
    }
}

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
            .WithDescription("只有提交导出的人和有管理作业权限的人能下载，别人看到的和不存在的一样，返回 404。作业没做完、文件还没登记完返回可重试的 409，作业失败或被取消返回 422。每次下载都有审计记录。");

    private static async Task<IResult> HandleAsync([Description("导出作业 ID")] Guid jobId, IQuerySender sender, CancellationToken ct)
    {
        var result = await sender.SendAsync(new GetAuditExportFileQuery(jobId), ct);
        return result.IsSuccess ? FileResults.Stream(result.Value, inline: false) : result.ToOk();
    }
}

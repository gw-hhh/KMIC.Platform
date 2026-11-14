using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Audit.Export;

namespace Kmic.Operations.Presentation.Audit;

internal sealed class ExportAuditEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/audit/exports", HandleAsync)
            .Sends<ExportAuditCommand>()
            .WithSummary("导出审计记录")
            .WithDescription("将指定时间段的审计记录导出为 CSV，筛选条件与审计列表相同。提交后返回 202 和 jobId，Location 指向下载地址；作业未完成时下载返回 409，可稍后重试。行数超限时作业失败，请缩小时间范围后重新导出。");

    private static async Task<IResult> HandleAsync(ExportAuditRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(
            new ExportAuditCommand(
                request.From,
                request.To,
                request.ActorUserId,
                request.ActualUserId,
                request.Module,
                request.Action,
                request.ResourceType,
                request.ResourceId),
            ct)).ToAccepted(jobId => $"/api/v1/operations/audit/exports/{jobId}/file");
}

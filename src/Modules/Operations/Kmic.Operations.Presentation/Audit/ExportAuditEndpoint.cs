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
            .WithDescription("把一段时间的审计记录导成 CSV 文件，筛选条件和审计列表一样。做成后台作业：返回 202，响应里的 jobId 是作业 ID，Location 指向结果文件，作业没做完时返回 409，稍后再取。行数超过上限时作业失败，要缩小时间范围。");

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

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
            .WithDescription("按列表相同条件异步导出 CSV，返回 202 与 jobId（Location 指向下载地址）；行超限时作业失败，须缩小时间范围");

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

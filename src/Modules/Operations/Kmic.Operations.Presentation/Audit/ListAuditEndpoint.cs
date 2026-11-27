using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Audit.List;

namespace Kmic.Operations.Presentation.Audit;

internal sealed class ListAuditEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/audit", HandleAsync)
            .Sends<ListAuditQuery>()
            .WithSummary("查询审计记录")
            .WithDescription("时间范围必填（最长 93 天），按发生时间倒序游标分页（传 nextCursor 到 after）；不按数据范围过滤，查询记录读取审计");

    private static async Task<IResult> HandleAsync([AsParameters] ListAuditRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(
            new ListAuditQuery(
                request.From,
                request.To,
                request.ActorUserId,
                request.ActualUserId,
                request.Module,
                request.Action,
                request.ResourceType,
                request.ResourceId,
                new CursorRequest(request.After, request.Limit)),
            ct)).ToOk();
}

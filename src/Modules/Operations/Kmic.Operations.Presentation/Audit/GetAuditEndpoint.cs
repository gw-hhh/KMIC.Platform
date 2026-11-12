using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Audit.Get;
using Kmic.Operations.Domain.Audit;

namespace Kmic.Operations.Presentation.Audit;

internal sealed class GetAuditEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/audit/{id}", HandleAsync)
            .Sends<GetAuditQuery>()
            .WithSummary("查询一条审计记录的详情")
            .WithDescription("含字段级变更与补充说明；操作人、角色的名称实时解析，已经不存在的显示为空。这个查询本身也会留下读取审计。");

    private static async Task<IResult> HandleAsync([Description("审计记录 ID")] AuditLogId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetAuditQuery(id), ct)).ToOk();
}

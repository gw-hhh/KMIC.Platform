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
            .WithSummary("查询审计记录详情")
            .WithDescription("包含字段变更和补充说明。操作人、角色名称实时查询，不存在时返回 null。查询本身也记录读取审计。");

    private static async Task<IResult> HandleAsync([Description("审计记录 ID")] AuditLogId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetAuditQuery(id), ct)).ToOk();
}

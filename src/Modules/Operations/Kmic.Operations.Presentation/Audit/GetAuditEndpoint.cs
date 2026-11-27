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
            .WithDescription("返回字段变更与补充说明（记录读取审计）；操作人与角色名实时查询，不存在时为 null");

    private static async Task<IResult> HandleAsync([Description("审计记录 ID")] AuditLogId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetAuditQuery(id), ct)).ToOk();
}

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authorization.Explain;

namespace Kmic.Identity.Presentation.Authorization;

internal sealed class ExplainAuthorizationEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/authorization/explain", HandleAsync)
            .Sends<ExplainAuthorizationQuery>()
            .WithSummary("授权诊断")
            .WithDescription("诊断用户在指定角色下的有效权限与登录可行性，传入范围对象时一并返回数据范围来源与可见组织数（记录读取审计；用户或角色不存在返回 404，范围对象无效返回 400）");

    private static async Task<IResult> HandleAsync([AsParameters] ExplainAuthorizationRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ExplainAuthorizationQuery(request.UserId, request.RoleId, request.ScopeObject), ct)).ToOk();
}

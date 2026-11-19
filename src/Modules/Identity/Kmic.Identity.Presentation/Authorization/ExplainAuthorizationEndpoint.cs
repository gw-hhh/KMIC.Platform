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
            .WithDescription("说明用户以某个角色登录时有哪些权限点；传了范围对象时，再说明这个对象的数据范围等级来自角色默认还是单独设置、组织起点来自授予锚点还是所属组织、能看到多少组织单元。用户不能以这个角色登录时返回原因。用户或角色不存在返回 404，范围对象不存在返回 400。查询会记录读取审计。");

    private static async Task<IResult> HandleAsync([AsParameters] ExplainAuthorizationRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ExplainAuthorizationQuery(request.UserId, request.RoleId, request.ScopeObject), ct)).ToOk();
}

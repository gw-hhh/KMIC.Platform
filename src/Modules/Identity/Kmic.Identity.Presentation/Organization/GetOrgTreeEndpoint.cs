using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Organization.Get;

namespace Kmic.Identity.Presentation.Organization;

internal sealed class GetOrgTreeEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/org-tree", HandleAsync)
            .Sends<GetOrgTreeQuery>()
            .WithSummary("查询整棵组织树")
            .WithDescription("登录即可用。平铺的全部单元（含已停用的），按上级分组、同一上级下按次序排好，前端按 parentId 拼成树；version 是修改组织树时要带的版本号。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetOrgTreeQuery(), ct)).ToOk();
}

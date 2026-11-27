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
            .WithSummary("查询完整组织树")
            .WithDescription("返回含停用单元的平铺列表（按上级分组、同级有序），供前端按 parentId 组树；修改组织树须回传返回的 version");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetOrgTreeQuery(), ct)).ToOk();
}

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
            .WithDescription("登录即可查询，返回包含停用单元的平铺列表，按上级分组、同级按顺序排列。前端通过 parentId 组装树；修改组织树时须提交返回的 version。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetOrgTreeQuery(), ct)).ToOk();
}

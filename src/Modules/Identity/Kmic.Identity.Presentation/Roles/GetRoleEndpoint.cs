using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Roles.Get;

namespace Kmic.Identity.Presentation.Roles;

internal sealed class GetRoleEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/roles/{id}", HandleAsync)
            .Sends<GetRoleQuery>()
            .WithSummary("查询角色详情")
            .WithDescription("权限点（已退役的标出）、数据范围配置、持有人数、版本号和当前用户可以执行的动作。");

    private static async Task<IResult> HandleAsync([Description("角色 ID")] RoleId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetRoleQuery(id), ct)).ToOk();
}

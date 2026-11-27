using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Organization.AssignUser;

namespace Kmic.Identity.Presentation.Organization;

internal sealed class AssignUserOrgUnitEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/users/{id}/org-unit", HandleAsync)
            .Sends<AssignUserOrgUnitCommand>()
            .WithSummary("设置用户所属组织")
            .WithDescription("设置用户所属组织（未指定角色授予锚点时作为“本组织/及下级”起点）；不影响已有业务数据的归属组织");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, AssignUserOrgUnitRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AssignUserOrgUnitCommand(id, request.ExpectedVersion, request.OrgUnitId), ct)).ToOk();
}

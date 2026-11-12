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
            .WithSummary("设置用户所属的组织单元")
            .WithDescription("直接改变用户的数据范围：“本组织”“本组织及下级”都从这里算起。调整之后他名下已有数据的所属组织不变。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, AssignUserOrgUnitRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AssignUserOrgUnitCommand(id, request.ExpectedVersion, request.OrgUnitId), ct)).ToOk();
}

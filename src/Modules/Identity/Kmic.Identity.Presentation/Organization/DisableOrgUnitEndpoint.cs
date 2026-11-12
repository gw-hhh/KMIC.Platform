using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Organization.Disable;

namespace Kmic.Identity.Presentation.Organization;

internal sealed class DisableOrgUnitEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/org-units/{id}/disable", HandleAsync)
            .Sends<DisableOrgUnitCommand>()
            .WithSummary("停用组织单元")
            .WithDescription("还有启用的下级、或有启用的用户归属于它时返回 409 Identity.OrgUnitInUse；根单元不能停用。");

    private static async Task<IResult> HandleAsync([Description("组织单元 ID")] OrgUnitId id, OrgUnitVersionRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new DisableOrgUnitCommand(request.ExpectedVersion, id), ct)).ToOk();
}

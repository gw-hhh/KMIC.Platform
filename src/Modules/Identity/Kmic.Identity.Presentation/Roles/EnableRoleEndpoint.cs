using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Roles.Enable;

namespace Kmic.Identity.Presentation.Roles;

internal sealed class EnableRoleEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/roles/{id}/enable", HandleAsync)
            .Sends<EnableRoleCommand>()
            .WithSummary("启用角色");

    private static async Task<IResult> HandleAsync([Description("角色 ID")] RoleId id, EnableRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new EnableRoleCommand(id, request.ExpectedVersion), ct)).ToOk();
}

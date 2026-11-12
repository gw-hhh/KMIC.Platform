using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Roles.SetDataScopes;

namespace Kmic.Identity.Presentation.Roles;

internal sealed class SetRoleDataScopesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/roles/{id}/data-scopes", HandleAsync)
            .Sends<SetRoleDataScopesCommand>()
            .WithSummary("整体替换角色的数据范围")
            .WithDescription("默认等级加各范围对象单独设置的等级；范围对象必须在目录里。");

    private static async Task<IResult> HandleAsync([Description("角色 ID")] RoleId id, SetRoleDataScopesRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetRoleDataScopesCommand(id, request.ExpectedVersion, request.DefaultLevel, request.Overrides), ct)).ToOk();
}

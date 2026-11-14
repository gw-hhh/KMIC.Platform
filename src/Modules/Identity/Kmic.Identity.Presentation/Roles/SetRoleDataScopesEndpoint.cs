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
            .WithSummary("替换角色数据范围")
            .WithDescription("包含默认范围等级及各对象单独设置的等级。所有数据范围对象都必须已在目录中声明。");

    private static async Task<IResult> HandleAsync([Description("角色 ID")] RoleId id, SetRoleDataScopesRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetRoleDataScopesCommand(id, request.ExpectedVersion, request.DefaultLevel, request.Overrides), ct)).ToOk();
}

using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.Disable;

namespace Kmic.Identity.Presentation.Users;

internal sealed class DisableUserEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/users/{id}/disable", HandleAsync)
            .Sends<DisableUserCommand>()
            .WithSummary("禁用用户")
            .WithDescription("禁用后该用户全部会话失效；不可禁用自己或最后一位可登录的受保护角色持有人");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, DisableUserRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new DisableUserCommand(id, request.ExpectedVersion, request.Reason), ct)).ToOk();
}

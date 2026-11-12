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
            .WithDescription("他的全部会话随之作废。不能禁用自己，也不能禁用最后一个启用的系统管理员。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, DisableUserRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new DisableUserCommand(id, request.ExpectedVersion, request.Reason), ct)).ToOk();
}

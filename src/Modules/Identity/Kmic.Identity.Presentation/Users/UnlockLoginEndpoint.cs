using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.UnlockLogin;

namespace Kmic.Identity.Presentation.Users;

internal sealed class UnlockLoginEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/users/{id}/unlock-login", HandleAsync)
            .Sends<UnlockLoginCommand>()
            .WithSummary("解锁登录")
            .WithDescription("清零这个账号的登录失败计数。登录限制只影响新的登录尝试，不影响用户已有的会话。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new UnlockLoginCommand(id), ct)).ToOk();
}

using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.ResetPassword;

namespace Kmic.Identity.Presentation.Users;

internal sealed class ResetUserPasswordEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/users/{id}/reset-password", HandleAsync)
            .Sends<ResetUserPasswordCommand>()
            .NoStore()
            .WithSummary("重置用户密码")
            .WithDescription("生成一次性临时密码，只在这次响应里出现；用户的全部会话作废，下次登录必须先改密码。不需要幂等键：重复提交会重新生成，之前的临时密码随之失效。持有受保护角色的用户，只有同样持有受保护角色的人才能重置。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetUserPasswordCommand(id), ct)).ToOk();
}

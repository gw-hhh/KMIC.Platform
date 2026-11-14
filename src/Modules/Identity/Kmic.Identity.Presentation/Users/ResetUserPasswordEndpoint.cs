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
            .WithDescription("生成临时密码，仅在本次响应中返回。该用户的全部会话失效，下次登录必须修改密码。无需幂等键，重复提交会生成新密码，旧临时密码随即失效。目标用户持有受保护角色时，操作者的当前角色也必须受保护。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetUserPasswordCommand(id), ct)).ToOk();
}

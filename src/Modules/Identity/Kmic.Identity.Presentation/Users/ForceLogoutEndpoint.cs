using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.ForceLogout;

namespace Kmic.Identity.Presentation.Users;

internal sealed class ForceLogoutEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/users/{id}/force-logout", HandleAsync)
            .Sends<ForceLogoutCommand>()
            .WithSummary("强制下线")
            .WithDescription("结束目标用户的未作废会话，包括以其身份创建或由其发起的模拟会话；可指定一个会话或处理全部。不能对自己执行此操作，请通过“我的登录设备”管理自己的会话。目标用户持有受保护角色时，操作者的当前角色也必须受保护。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, ForceLogoutRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ForceLogoutCommand(id, request.SessionId), ct)).ToOk();
}

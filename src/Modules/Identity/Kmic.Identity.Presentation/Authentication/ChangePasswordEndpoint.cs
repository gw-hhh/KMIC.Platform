using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication.ChangePassword;

namespace Kmic.Identity.Presentation.Authentication;

internal sealed class ChangePasswordEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/auth/change-password", HandleAsync)
            .Sends<ChangePasswordCommand>()
            .WithSummary("修改自己的密码")
            .WithDescription("已经有密码时要给旧密码；还没有设置过密码的用户（靠短信登录的）不要求。成功后其他会话全部作废，当前会话继续。");

    private static async Task<IResult> HandleAsync(ChangePasswordRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ChangePasswordCommand(request.OldPassword, request.NewPassword), ct)).ToNoContent();
}

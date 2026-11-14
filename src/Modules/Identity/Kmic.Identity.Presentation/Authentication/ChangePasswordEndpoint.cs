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
            .WithDescription("已有密码时须提供旧密码，包括管理员重置的临时密码；尚未设置密码时无需提供。成功后其他会话全部失效，当前会话保留。");

    private static async Task<IResult> HandleAsync(ChangePasswordRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ChangePasswordCommand(request.OldPassword, request.NewPassword), ct)).ToNoContent();
}

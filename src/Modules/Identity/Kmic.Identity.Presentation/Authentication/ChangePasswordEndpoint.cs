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
            .WithSummary("修改当前用户密码")
            .WithDescription("已有密码（含临时密码）时须传旧密码，未设密码时可空；成功后仅保留当前会话，其余会话全部失效");

    private static async Task<IResult> HandleAsync(ChangePasswordRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ChangePasswordCommand(request.OldPassword, request.NewPassword), ct)).ToNoContent();
}

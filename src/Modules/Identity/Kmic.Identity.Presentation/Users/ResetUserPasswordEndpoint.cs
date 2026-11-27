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
            .WithDescription("生成仅在本次响应返回的临时密码，全部会话失效且下次登录须改密（无需幂等键，重复提交将覆盖旧临时密码）；目标持受保护角色时操作者当前角色也须受保护");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetUserPasswordCommand(id), ct)).ToOk();
}

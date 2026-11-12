using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Impersonation.Start;

namespace Kmic.Identity.Presentation.Impersonation;

internal sealed class StartImpersonationEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/users/{id}/impersonation", HandleAsync)
            .Sends<StartImpersonationCommand>()
            .NoStore()
            .WithSummary("开始模拟身份")
            .WithDescription("创建一个独立的模拟会话，管理员自己的会话不受影响。响应只有访问令牌，没有刷新令牌，有效期就是会话上限（默认 1 小时）；用它调用接口时，操作人是被模拟的用户，审计记录里另有实际操作人。默认只读，另有“模拟身份时允许写入”权限点时可以修改数据。不能模拟自己、系统账号和受保护角色的持有人。不需要幂等键。");

    private static async Task<IResult> HandleAsync([Description("被模拟的用户 ID")] UserId id, StartImpersonationRequest request, HttpContext http, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new StartImpersonationCommand(id, request.RoleId, request.Reason, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent.ToString()), ct)).ToOk();
}

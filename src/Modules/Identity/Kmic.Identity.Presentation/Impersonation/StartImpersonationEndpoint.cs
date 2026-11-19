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
            .WithDescription("创建独立的模拟会话，不影响管理员原会话。仅返回访问令牌，无刷新令牌，有效期为会话上限（默认 1 小时）。请求以目标用户身份执行，审计另记实际操作人。默认只读，具备“模拟身份时允许写入”权限时可修改数据。不能模拟自己、系统账号或受保护角色持有人，也不能在模拟会话中再次模拟。");

    private static async Task<IResult> HandleAsync([Description("被模拟的用户 ID")] UserId id, StartImpersonationRequest request, HttpContext http, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new StartImpersonationCommand(id, request.RoleId, request.Reason, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent.ToString()), ct)).ToOk();
}

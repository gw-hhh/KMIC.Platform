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
            .WithDescription("开启独立模拟会话（不影响原会话），仅返回访问令牌且默认只读（具备写入权限时可改数据）；不可模拟自己、系统账号、受保护角色持有人或二次模拟");

    private static async Task<IResult> HandleAsync([Description("被模拟的用户 ID")] UserId id, StartImpersonationRequest request, HttpContext http, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new StartImpersonationCommand(id, request.RoleId, request.Reason, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent.ToString()), ct)).ToOk();
}

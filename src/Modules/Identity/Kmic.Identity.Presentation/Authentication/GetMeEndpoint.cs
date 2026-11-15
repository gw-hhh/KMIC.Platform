using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Authentication;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication.GetMe;

namespace Kmic.Identity.Presentation.Authentication;

internal sealed class GetMeEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/me", HandleAsync)
            .Sends<GetMeQuery>()
            .AllowWhenRoleUnavailable()
            .WithSummary("查询当前用户信息")
            .WithDescription("返回用户资料、当前角色及已获授的启用角色、当前权限、授权版本、密码修改要求、身份模拟状态和业务时区。当前角色不可用时仍可调用，供前端引导重新选择角色。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetMeQuery(), ct)).ToOk();
}

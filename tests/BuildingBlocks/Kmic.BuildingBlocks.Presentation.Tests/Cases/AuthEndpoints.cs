using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Presentation.Authentication;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Cases;

/// <summary>
/// 令牌验证与访问上下文的测试端点：回报请求里的当前身份
/// </summary>
internal static class AuthEndpoints
{
    public static readonly Type[] All = [typeof(WhoAmIEndpoint), typeof(RoleUnavailableOkEndpoint), typeof(PublicWhoAmIEndpoint)];

    internal sealed record Identity(string? UserId, string? RoleId, string? SessionId, bool IsSystem, string Origin);

    // 所有已登录用户可用；当前角色被收回后被拦截
    internal sealed class WhoAmIEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/whoami", Handle).Sends<MeQuery>().WithSummary("当前身份");

        private static Identity Handle(IExecutionContext execution) => Describe(execution);
    }

    // 声明了当前角色不可用时仍放行，对应 me 和 switch-role
    internal sealed class RoleUnavailableOkEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/whoami-allowed", Handle).Sends<MeQuery>().AllowWhenRoleUnavailable().WithSummary("当前身份，角色不可用时也可用");

        private static Identity Handle(IExecutionContext execution) => Describe(execution);
    }

    // 匿名端点（对应登录、刷新）：带着令牌的请求也能进来
    internal sealed class PublicWhoAmIEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/whoami-public", Handle).Sends<PublicQuery>().AllowAnonymous().WithSummary("匿名可用");

        private static Identity Handle(IExecutionContext execution) => Describe(execution);
    }

    private static Identity Describe(IExecutionContext execution) => new(
        execution.User.UserId?.Value.ToString(),
        execution.User.RoleId?.Value.ToString(),
        execution.User.SessionId?.Value.ToString(),
        execution.User.IsSystem,
        execution.Origin.ToString());
}

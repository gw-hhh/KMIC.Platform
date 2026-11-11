using Kmic.BuildingBlocks.Presentation.Endpoints;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Cases;

/// <summary>
/// 违反端点声明规则的端点：每个只犯一个错，启动校验要逐条指出
/// </summary>
internal static class BrokenEndpoints
{
    internal sealed class NoSendsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) => group.MapGet("/no-sends", () => "x").WithSummary("没有声明");
    }

    internal sealed class TwoSendsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/two-sends", () => "x").Sends<GetThingQuery>().Sends<MeQuery>().WithSummary("声明了两个");
    }

    internal sealed class SendsSystemOnlyEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) => group.MapPost("/system", () => "x").Sends<SystemCommand>().WithSummary("系统命令");
    }

    internal sealed class SendsContractQueryEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) => group.MapGet("/contract", () => "x").Sends<ContractThingQuery>().WithSummary("契约查询");
    }

    // 消息允许匿名，端点没有 AllowAnonymous
    internal sealed class ProtectedEndpointForAnonymousMessage : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) => group.MapGet("/public-a", () => "x").Sends<PublicQuery>().WithSummary("匿名消息");
    }

    // 端点 AllowAnonymous，消息只要求登录
    internal sealed class AnonymousEndpointForProtectedMessage : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) => group.MapGet("/public-b", () => "x").Sends<MeQuery>().AllowAnonymous().WithSummary("匿名端点");
    }

    // 两边都匿名，一致
    internal sealed class AnonymousEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) => group.MapGet("/public-c", () => "x").Sends<PublicQuery>().AllowAnonymous().WithSummary("匿名");
    }

    internal sealed class DuplicateOneEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) => group.MapGet("/dup/{id}", (string id) => id).Sends<MeQuery>().WithSummary("重复一");
    }

    internal sealed class DuplicateTwoEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) => group.MapGet("/dup/{thingId}", (string thingId) => thingId).Sends<MeQuery>().WithSummary("重复二");
    }
}

using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Shouldly;
using static Kmic.BuildingBlocks.Presentation.Tests.Cases.BrokenEndpoints;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Endpoints;

/// <summary>
/// 端点声明的启动校验：每一条都要在宿主启动时拒绝，而不是到请求时才发现
/// </summary>
public sealed class EndpointDeclarationTests
{
    [Fact]
    public async Task ValidEndpoints_Start()
    {
        await using var api = await ApiHost.StartAsync([ThingEndpoints.Module()]);

        api.App.Services.ShouldNotBeNull();
    }

    [Fact]
    public async Task EndpointWithoutSends_IsRejected()
    {
        var problem = await StartFailsAsync(new ListedModule("Broken", typeof(NoSendsEndpoint)));

        problem.ShouldContain("/api/v1/broken/no-sends");
        problem.ShouldContain("没有声明 .Sends<T>()");
    }

    [Fact]
    public async Task EndpointSendingTwoMessages_IsRejected()
    {
        var problem = await StartFailsAsync(new ListedModule("Broken", typeof(TwoSendsEndpoint)));

        problem.ShouldContain("一个端点只能发送一个消息");
    }

    [Fact]
    public async Task EndpointSendingSystemOnlyMessage_IsRejected()
    {
        var problem = await StartFailsAsync(new ListedModule("Broken", typeof(SendsSystemOnlyEndpoint)));

        problem.ShouldContain("SystemCommand");
        problem.ShouldContain("[SystemOnly]");
    }

    [Fact]
    public async Task EndpointSendingContractQuery_IsRejected()
    {
        var problem = await StartFailsAsync(new ListedModule("Broken", typeof(SendsContractQueryEndpoint)));

        problem.ShouldContain("ContractThingQuery");
        problem.ShouldContain("[ContractQuery]");
    }

    [Fact]
    public async Task AnonymousMessageBehindProtectedEndpoint_IsRejected()
    {
        var problem = await StartFailsAsync(new ListedModule("Broken", typeof(ProtectedEndpointForAnonymousMessage)));

        problem.ShouldContain("PublicQuery 标了 [AllowAnonymous]，端点也要 .AllowAnonymous()");
    }

    [Fact]
    public async Task AnonymousEndpointForAuthenticatedMessage_IsRejected()
    {
        var problem = await StartFailsAsync(new ListedModule("Broken", typeof(AnonymousEndpointForProtectedMessage)));

        problem.ShouldContain("端点 .AllowAnonymous()，但 MeQuery 没有标 [AllowAnonymous]");
    }

    [Fact]
    public async Task AnonymousOutsideIdentityModule_IsRejected()
    {
        // 测试程序集里的消息属于 BuildingBlocks 模块，不是 Identity
        var problem = await StartFailsAsync(new ListedModule("Broken", typeof(AnonymousEndpoint)));

        problem.ShouldContain("匿名访问只允许在 Identity 模块中");
    }

    [Fact]
    public async Task TwoEndpointsOnTheSameRoute_AreRejected()
    {
        // {id} 和 {thingId} 对路由匹配来说是同一条路由
        var problem = await StartFailsAsync(new ListedModule("Broken", typeof(DuplicateOneEndpoint), typeof(DuplicateTwoEndpoint)));

        problem.ShouldContain("GET /api/v1/broken/dup/{} 被 2 个端点同时声明");
    }

    [Fact]
    public async Task MessageMissingFromTheCatalog_IsRejected()
    {
        var host = ApiHost.Create([ThingEndpoints.Module()], messages: [typeof(MeQuery)]);
        await using var _ = host;

        var thrown = await Should.ThrowAsync<DeclarationException>(host.StartAsync());

        thrown.Problems.ShouldContain(p => p.Contains("GetThingQuery", StringComparison.Ordinal) && p.Contains("不在用例目录中", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AllProblemsAreReportedTogether()
    {
        var host = ApiHost.Create([new ListedModule("Broken", typeof(NoSendsEndpoint), typeof(TwoSendsEndpoint), typeof(SendsSystemOnlyEndpoint))]);
        await using var _ = host;

        var thrown = await Should.ThrowAsync<DeclarationException>(host.StartAsync());

        thrown.Problems.Count.ShouldBe(3);
    }

    private static async Task<string> StartFailsAsync(ModuleEndpoints module)
    {
        var host = ApiHost.Create([module]);
        await using var _ = host;

        var thrown = await Should.ThrowAsync<DeclarationException>(host.StartAsync());

        return string.Join(Environment.NewLine, thrown.Problems);
    }
}

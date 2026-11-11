using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Microsoft.AspNetCore.Http.Metadata;
using Shouldly;
using static Kmic.BuildingBlocks.Presentation.Tests.Cases.BrokenEndpoints;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Endpoints;

public sealed class EndpointMappingTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("Identity", "identity")]
    [InlineData("Sample", "sample")]
    [InlineData("AfterSales", "after-sales")]
    [InlineData("HrAndPayroll", "hr-and-payroll")]
    public void ModuleSegment_IsLowercaseWithHyphens(string module, string expected) =>
        RouteNames.Segment(module).ShouldBe(expected);

    [Fact]
    public async Task Endpoints_AreMappedUnderApiV1AndModuleSegment()
    {
        await using var api = await ApiHost.StartAsync([ThingEndpoints.Module("AfterSales")]);
        api.Script.On<MeQuery, string>((_, _) => "alice");

        (await api.Client.GetAsync("/api/v1/after-sales/me", Ct)).StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        (await api.Client.GetAsync("/api/v1/samples/me", Ct)).StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        (await api.Client.GetAsync("/me", Ct)).StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ModuleGroup_RequiresLogin()
    {
        await using var api = await ApiHost.StartAsync([ThingEndpoints.Module()]);
        api.Script.On<MeQuery, string>((_, _) => "alice");

        var response = await api.Anonymous.GetAsync("/api/v1/samples/me", Ct);

        await response.ShouldBeProblemAsync(401, "Common.Unauthenticated");
    }

    [Fact]
    public async Task AnonymousEndpoint_NeedsNoLogin()
    {
        // 校验要求匿名只在 Identity 模块，测试里的消息属于 BuildingBlocks，所以用只含一个匿名端点的模块并放宽不了校验：
        // 这里直接不经校验地映射，只验证 AllowAnonymous 在路由组之上生效
        await using var api = await ApiHost.StartAsync(
            [new ListedModule("Identity", typeof(AnonymousEndpoint))],
            skipValidation: true);
        api.Script.On<PublicQuery, string>((_, _) => "hello");

        var response = await api.Anonymous.GetAsync("/api/v1/identity/public-c", Ct);

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Endpoints_AreTaggedWithTheirModule()
    {
        await using var api = await ApiHost.StartAsync([ThingEndpoints.Module("Sales")]);

        var endpoints = api.App.Services.GetServices<EndpointDataSource>().SelectMany(s => s.Endpoints).OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<ModuleMetadata>() is not null)
            .ToList();

        endpoints.Count.ShouldBe(ThingEndpoints.All.Length);
        endpoints.ShouldAllBe(e => e.Metadata.GetMetadata<ITagsMetadata>()!.Tags.Contains("Sales"));
        endpoints.ShouldAllBe(e => e.Metadata.GetMetadata<SendsMetadata>() != null);
    }

    [Fact]
    public void DefaultDiscovery_FindsEveryConcreteEndpointInTheAssembly()
    {
        var found = new ScanningModule().Found().ToList();

        found.ShouldContain(typeof(ThingEndpoints.GetThingEndpoint));
        found.ShouldContain(typeof(NoSendsEndpoint));
        found.ShouldNotContain(typeof(AbstractEndpoint));
        found.ShouldAllBe(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsAbstract);
    }

    [Fact]
    public void ModuleRegisteredTwice_IsRejectedAtMapping()
    {
        var modules = new ModuleEndpoints[] { ThingEndpoints.Module("Samples"), new ListedModule("samples") };

        var thrown = Should.Throw<DeclarationException>(() => ApiHost.Create(modules));

        thrown.Problems.ShouldHaveSingleItem().ShouldContain("Samples");
    }

    private sealed class ScanningModule : ModuleEndpoints
    {
        public override string Module => "Scan";

        public IEnumerable<Type> Found() => FindEndpointTypes();
    }

    private abstract class AbstractEndpoint : IEndpoint
    {
        public abstract void Map(IEndpointRouteBuilder group);
    }
}

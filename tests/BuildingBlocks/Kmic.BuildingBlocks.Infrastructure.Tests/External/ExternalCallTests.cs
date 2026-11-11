using System.Net;
using Kmic.BuildingBlocks.Infrastructure.External;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.External;

/// <summary>
/// 客户端的超时、重试和命令里的守卫。“在命令里”用一个真实的命令，处理器里调用客户端
/// </summary>
public sealed class ExternalCallTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task OutsideCommand_CallGoesThrough()
    {
        var fake = new FakePartnerServer(HttpStatusCode.OK);
        await using var provider = Build(fake);

        var status = await provider.GetRequiredService<IPartnerClient>().GetStatusAsync(Ct);

        status.ShouldBe(HttpStatusCode.OK);
        fake.Calls.ShouldBe(1);
    }

    [Fact]
    public async Task InsideCommand_Rejected_BeforeAnythingIsSent()
    {
        await using var harness = new ProbeHarness(database.ConnectionString);
        await using var work = harness.Begin(_alice);

        var thrown = await Should.ThrowAsync<InvalidOperationException>(async () => await work.Sender.SendAsync(new CallPartnerCommand(), Ct));

        thrown.Message.ShouldContain("命令里不能调用外部系统");
        harness.PartnerServer.Calls.ShouldBe(0);
    }

    [Fact]
    public async Task ReadRequest_RetriedOnServerError()
    {
        var fake = new FakePartnerServer(HttpStatusCode.InternalServerError, HttpStatusCode.OK);
        await using var provider = Build(fake, o => o.RetryReads = true);

        var status = await provider.GetRequiredService<IPartnerClient>().GetStatusAsync(Ct);

        status.ShouldBe(HttpStatusCode.OK);
        fake.Calls.ShouldBe(2);
    }

    [Fact]
    public async Task WriteRequest_NotRetried()
    {
        var fake = new FakePartnerServer(HttpStatusCode.InternalServerError, HttpStatusCode.OK);
        await using var provider = Build(fake, o => o.RetryReads = true);

        var status = await provider.GetRequiredService<IPartnerClient>().PostAsync(Ct);

        status.ShouldBe(HttpStatusCode.InternalServerError);
        fake.Calls.ShouldBe(1);
    }

    private static ServiceProvider Build(FakePartnerServer fake, Action<ExternalClientOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddKmicHttpClient<IPartnerClient, PartnerClient>("Partner", configure)
            .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://partner.example.com"))
            .ConfigurePrimaryHttpMessageHandler(() => fake);
        return services.BuildServiceProvider();
    }
}

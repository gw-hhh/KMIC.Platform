using System.Net;
using Kmic.BuildingBlocks.Presentation.Limits;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Shouldly;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Limits;

public sealed class ConcurrencyLimitTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Heavy_RejectsBeyondThePerInstanceLimit_AndReleasesWhenDone()
    {
        var gate = new RequestGate();
        await using var api = await StartAsync(gate, heavyConcurrency: 2);

        var first = api.Client.GetAsync("/api/v1/samples/heavy", Ct);
        var second = api.Client.GetAsync("/api/v1/samples/heavy", Ct);
        await gate.WaitEnteredAsync(2, Ct);

        var rejected = await api.Client.GetAsync("/api/v1/samples/heavy", Ct);

        var body = await rejected.ShouldBeProblemAsync(429, "Common.TooBusy");
        body.GetProperty("retryable").GetBoolean().ShouldBeTrue();
        rejected.Headers.GetValues("Retry-After").ShouldHaveSingleItem().ShouldNotBeNullOrWhiteSpace();

        gate.Release();
        (await first).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await second).StatusCode.ShouldBe(HttpStatusCode.OK);

        // 请求处理完成后立即归还并发许可
        (await api.Client.GetAsync("/api/v1/samples/heavy", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Heavy_DoesNotQueue_TheRejectionIsImmediate()
    {
        var gate = new RequestGate();
        await using var api = await StartAsync(gate, heavyConcurrency: 1);
        var running = api.Client.GetAsync("/api/v1/samples/heavy", Ct);
        await gate.WaitEnteredAsync(1, Ct);

        // 放行前第二个请求即已返回结果，表明未进入排队等待
        var rejected = await api.Client.GetAsync("/api/v1/samples/heavy", Ct);

        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        running.IsCompleted.ShouldBeFalse();
        gate.Release();
        await running;
    }

    [Fact]
    public async Task HeavyPerUser_AllowsOneAtATimePerUser()
    {
        var gate = new RequestGate();
        await using var api = await StartAsync(gate, heavyConcurrency: 10);
        using var bob = api.ClientFor("bob");

        var alice = api.Client.GetAsync("/api/v1/samples/heavy-per-user", Ct);
        await gate.WaitEnteredAsync(1, Ct);

        var aliceAgain = await api.Client.GetAsync("/api/v1/samples/heavy-per-user", Ct);
        var bobRequest = bob.GetAsync("/api/v1/samples/heavy-per-user", Ct);
        await gate.WaitEnteredAsync(1, Ct);

        await aliceAgain.ShouldBeProblemAsync(429, "Common.TooBusy");
        gate.Release();
        (await alice).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await bobRequest).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task BothPolicies_ReleaseTheFirstPermitWhenTheSecondIsRefused()
    {
        var gate = new RequestGate();
        await using var api = await StartAsync(gate, heavyConcurrency: 2);
        using var bob = api.ClientFor("bob");

        var alice = api.Client.GetAsync("/api/v1/samples/heavy-both", Ct);
        await gate.WaitEnteredAsync(1, Ct);

        // 第二个请求获取实例名额后被单用户并发限制拒绝：必须立即归还实例名额，否则后续 bob 的请求将被误拒
        var aliceAgain = await api.Client.GetAsync("/api/v1/samples/heavy-both", Ct);
        var bobRequest = bob.GetAsync("/api/v1/samples/heavy-both", Ct);
        await gate.WaitEnteredAsync(1, Ct);

        aliceAgain.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        gate.Release();
        (await alice).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await bobRequest).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EndpointsWithoutADeclaration_AreNotAffectedWhileHeavyIsFull()
    {
        var gate = new RequestGate();
        await using var api = await StartAsync(gate, heavyConcurrency: 1);
        var running = api.Client.GetAsync("/api/v1/samples/heavy", Ct);
        await gate.WaitEnteredAsync(1, Ct);

        var light = await api.Client.GetAsync("/api/v1/samples/light", Ct);

        light.StatusCode.ShouldBe(HttpStatusCode.OK);
        gate.Release();
        await running;
    }


    [Fact]
    public void UnknownPolicy_IsRejectedWhenDeclared()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();

        var thrown = Should.Throw<ArgumentException>(() => app.MapGet("/x", () => "x").RequireConcurrency("Huge"));

        thrown.Message.ShouldContain("Huge");
    }

    [Fact]
    public void NoPolicy_IsRejectedWhenDeclared()
    {
        var builder = WebApplication.CreateBuilder();
        using var app = builder.Build();

        Should.Throw<ArgumentException>(() => app.MapGet("/x", () => "x").RequireConcurrency());
    }

    private static async Task<ApiHost> StartAsync(RequestGate gate, int heavyConcurrency)
    {
        var configuration = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["Kmic:Http:HeavyConcurrency"] = heavyConcurrency.ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        var api = await ApiHost.StartAsync(
            [new ListedModule("Samples", LimitEndpoints.All)],
            services: s => s.AddSingleton(gate),
            configuration: configuration);
        api.Script.On<MeQuery, string>((_, _) => "alice");
        return api;
    }
}

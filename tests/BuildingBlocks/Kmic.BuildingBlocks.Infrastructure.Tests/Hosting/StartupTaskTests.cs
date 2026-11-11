using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Hosting;

public sealed class StartupTaskTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Start_RunsInOrder_ThenMarksCompleted()
    {
        var log = new List<string>();
        await using var harness = Harness(log, ("Probe.Second", 10, true), ("Framework.Early", -100, true), ("Probe.First", 0, true));
        var state = harness.Services.GetRequiredService<StartupState>();

        await harness.StartHostedServicesAsync(Ct);

        log.ShouldBe(["Framework.Early", "Probe.First", "Probe.Second"]);
        state.IsCompleted.ShouldBeTrue();

        // 框架自带的路由登记排在最前面
        await using var routes = database.DataSource.CreateCommand("""SELECT count(*) FROM "Operations"."MessageRoute" WHERE "Subscription" = @name""");
        routes.Parameters.AddWithValue("name", ProbeHarness.MirrorSubscription);
        (await routes.ExecuteScalarAsync(Ct)).ShouldBe(1L);
        await harness.StopHostedServicesAsync(Ct);
    }

    [Fact]
    public async Task TaskFails_StartThrows_LaterTasksSkipped_BackgroundNeverStarts()
    {
        var log = new List<string>();
        await using var harness = Harness(log, ("Probe.Broken", 0, false), ("Probe.After", 1, true));

        var thrown = await Should.ThrowAsync<InvalidOperationException>(() => harness.StartHostedServicesAsync(Ct));

        thrown.Message.ShouldContain("Probe.Broken");
        log.ShouldBe(["Probe.Broken"]);
        harness.Services.GetRequiredService<StartupState>().IsCompleted.ShouldBeFalse();
    }

    [Theory]
    [InlineData("Probe.Negative", -1)]
    [InlineData("Framework.Positive", 0)]
    public async Task WrongOrderForKind_Rejected(string name, int order)
    {
        await using var harness = Harness([], (name, order, true));

        var thrown = await Should.ThrowAsync<DeclarationException>(() => harness.StartHostedServicesAsync(Ct));

        thrown.Problems.ShouldHaveSingleItem().ShouldContain(name);
    }

    [Fact]
    public async Task DuplicateName_Rejected()
    {
        await using var harness = Harness([], ("Probe.Same", 0, true), ("Probe.Same", 1, true));

        var thrown = await Should.ThrowAsync<DeclarationException>(() => harness.StartHostedServicesAsync(Ct));

        thrown.Problems.ShouldHaveSingleItem().ShouldContain("重复");
    }

    private ProbeHarness Harness(List<string> log, params (string Name, int Order, bool Succeeds)[] tasks) =>
        new(database.ConnectionString, services: s =>
        {
            foreach (var (name, order, succeeds) in tasks)
            {
                s.AddScoped<IStartupTask>(_ => new RecordingTask(name, order, succeeds, log));
            }
        });

    private sealed class RecordingTask(string name, int order, bool succeeds, List<string> log) : IStartupTask
    {
        public string Name => name;

        public int Order => order;

        public ValueTask<Result> ExecuteAsync(CancellationToken ct)
        {
            log.Add(name);
            return ValueTask.FromResult(succeeds ? Result.Success() : Error.Conflict("Probe.StartupFailed", "初始化失败"));
        }
    }
}

using System.Diagnostics.Metrics;
using System.Globalization;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Kmic.BuildingBlocks.Testing.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// 容量保护的采样与判断：真实的数据库大小、各模块 Outbox 与 Inbox 的未完成量，容量用替身。判断时间用 SQL 改，周期才可以控制
/// </summary>
public sealed class AdmissionSamplerTests : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncLifetime
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _alice = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ClassDatabase<ProbeTemplate> _database;
    private readonly FakeCapacity _capacity = new();
    private readonly ProbeHarness _harness;
    private MetricRecorder _metrics = null!;

    public AdmissionSamplerTests(ClassDatabase<ProbeTemplate> database)
    {
        _database = database;
        _harness = new ProbeHarness(
            database.ConnectionString,
            messaging: o =>
            {
                o.Admission.MinUnfinishedBytes = 1;
                o.Admission.LowWaterBytes = 0;
            },
            services: s =>
            {
                s.AddSingleton<IMessagingCapacitySource>(_capacity);
                s.AddKmicMessagingAdmission();
            });
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private AdmissionSampler Sampler => _harness.Services.GetRequiredService<AdmissionSampler>();

    private MessagingAdmission Admission => _harness.Services.GetRequiredService<MessagingAdmission>();

    public async ValueTask InitializeAsync()
    {
        foreach (var table in new[] { "Probe.OutboxMessage", "Probe.InboxMessage", "Echo.OutboxMessage", "Echo.InboxMessage", "Operations.MessagingAdmission" })
        {
            var parts = table.Split('.');
            await ExecuteAsync($"DELETE FROM \"{parts[0]}\".\"{parts[1]}\"");
        }

        _harness.Access.Register(_alice);
        _metrics = new MetricRecorder(_harness.Services.GetRequiredService<IMeterFactory>());
    }

    public async ValueTask DisposeAsync()
    {
        _metrics.Dispose();
        await _harness.DisposeAsync();
    }

    [Fact]
    public async Task NoCapacitySetting_WritesNormal_AndKeepsJudging()
    {
        _capacity.Value = null;

        await Sampler.CycleAsync(Ct);

        var row = await ReadRowAsync();
        row.State.ShouldBe((short)0);
        row.CapacityBytes.ShouldBeNull();
        Admission.State.ShouldBe(AdmissionState.Normal);
    }

    [Fact]
    public async Task SpaceBetweenTheLines_WithQueuedMessages_BecomesSoft()
    {
        await QueueMessageAsync();
        await SetCapacityForUsageAsync(0.90);

        await Sampler.CycleAsync(Ct);

        var row = await ReadRowAsync();
        row.State.ShouldBe((short)1);
        row.UnfinishedBytes.ShouldBeGreaterThan(0);
        Admission.State.ShouldBe(AdmissionState.Soft);
    }

    [Fact]
    public async Task SpaceBetweenTheLines_WithoutQueuedMessages_StaysNormal()
    {
        await SetCapacityForUsageAsync(0.90);

        await Sampler.CycleAsync(Ct);

        (await ReadRowAsync()).State.ShouldBe((short)0);
    }

    [Fact]
    public async Task SpaceOverTheHardLine_BecomesHard_EvenWithoutQueuedMessages()
    {
        await SetCapacityForUsageAsync(0.97);

        await Sampler.CycleAsync(Ct);

        (await ReadRowAsync()).State.ShouldBe((short)2);
        Admission.State.ShouldBe(AdmissionState.Hard);
    }

    [Fact]
    public async Task Soft_RecoversAfterTwoPeriodsWithAnEmptyQueue()
    {
        await QueueMessageAsync();
        await SetCapacityForUsageAsync(0.90);
        await Sampler.CycleAsync(Ct);
        await ClearQueueAsync();

        await AgeSampleAsync();
        await Sampler.CycleAsync(Ct);
        (await ReadRowAsync()).State.ShouldBe((short)1, "队列刚排空，还要再观察一个周期");

        await AgeSampleAsync();
        await Sampler.CycleAsync(Ct);
        (await ReadRowAsync()).State.ShouldBe((short)0);
        Admission.State.ShouldBe(AdmissionState.Normal);
    }

    [Fact]
    public async Task EvaluatedAgainInsideTheSamePeriod_IsSkipped()
    {
        await QueueMessageAsync();
        await SetCapacityForUsageAsync(0.90);
        await Sampler.CycleAsync(Ct);
        await ClearQueueAsync();

        // 不改判断时间：另一个实例刚判断过，这一轮不能再算一次，否则低水位的周期数会多记
        await Sampler.CycleAsync(Ct);

        (await ReadRowAsync()).LowWaterPeriods.ShouldBe(0);
    }

    [Fact]
    public async Task TwoInstancesEvaluatingTogether_CountOnePeriod()
    {
        await QueueMessageAsync();
        await SetCapacityForUsageAsync(0.90);
        await Sampler.CycleAsync(Ct);
        await ClearQueueAsync();
        await AgeSampleAsync();

        await Task.WhenAll(Sampler.CycleAsync(Ct), Sampler.CycleAsync(Ct));

        (await ReadRowAsync()).LowWaterPeriods.ShouldBe(1);
    }

    [Fact]
    public async Task Gauges_ReportStateAndAges()
    {
        await SetCapacityForUsageAsync(0.97);
        await Sampler.CycleAsync(Ct);

        _metrics.Clear();
        _metrics.Collect();

        Gauge("kmic.messaging.admission.state").ShouldBe(2);
        Gauge("kmic.messaging.admission.sample_age").ShouldBeGreaterThanOrEqualTo(0);
        Gauge("kmic.messaging.admission.capacity_age").ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Gauges_NothingReadYet_ReportNothing()
    {
        _ = Sampler;

        _metrics.Clear();
        _metrics.Collect();

        _metrics.All.Where(m => m.Instrument.StartsWith("kmic.messaging.admission.", StringComparison.Ordinal)).ShouldBeEmpty();
    }

    private double Gauge(string name) => _metrics.All.Where(m => string.Equals(m.Instrument, name, StringComparison.Ordinal)).ShouldHaveSingleItem().Value;

    private async Task SetCapacityForUsageAsync(double ratio)
    {
        var size = await ScalarAsync("SELECT pg_database_size(current_database())");
        _capacity.Value = new MessagingCapacity((long)(size / ratio), _harness.Time.GetUtcNow());
    }

    private async Task QueueMessageAsync()
    {
        var ticket = await _harness.AddAsync(_alice, Ticket.Create(TicketNumbers.Next(), _alice.UserId, _org["总部"]));
        await using var work = _harness.Begin(_alice);
        (await work.Sender.SendAsync(new ShareTicketCommand(ticket.Id, ["排队"]), Ct)).ShouldBeSuccess();
    }

    private Task ClearQueueAsync() => ExecuteAsync("DELETE FROM \"Probe\".\"OutboxMessage\"");

    private Task AgeSampleAsync() => ExecuteAsync("UPDATE \"Operations\".\"MessagingAdmission\" SET \"SampledAt\" = \"SampledAt\" - interval '10 minutes'");

    private async Task<AdmissionRow> ReadRowAsync()
    {
        await using var command = _database.DataSource.CreateCommand(
            "SELECT \"State\", \"LowWaterPeriods\", \"UnfinishedBytes\", \"CapacityBytes\" FROM \"Operations\".\"MessagingAdmission\" WHERE \"Id\" = 1");
        await using var reader = await command.ExecuteReaderAsync(Ct);
        (await reader.ReadAsync(Ct)).ShouldBeTrue("判断之后应该有一行状态");
        long? capacity = await reader.IsDBNullAsync(3, Ct) ? null : reader.GetInt64(3);
        return new AdmissionRow(reader.GetInt16(0), reader.GetInt32(1), reader.GetInt64(2), capacity);
    }

    private async Task<long> ScalarAsync(string sql)
    {
        await using var command = _database.DataSource.CreateCommand(sql);
        return Convert.ToInt64(await command.ExecuteScalarAsync(Ct), CultureInfo.InvariantCulture);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using var command = _database.DataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private sealed record AdmissionRow(short State, int LowWaterPeriods, long UnfinishedBytes, long? CapacityBytes);

    private sealed class FakeCapacity : IMessagingCapacitySource
    {
        public MessagingCapacity? Value { get; set; }

        public Task<MessagingCapacity?> ReadAsync(CancellationToken ct) => Task.FromResult(Value);
    }
}

using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// 每个测试用自己的消息名和订阅名，同一个库里的路由互不干扰
/// </summary>
public sealed class MessageRouteTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>
{
    private readonly MessageRoutes _routes = new(database.DataSource);
    private readonly string _suffix = Guid.CreateVersion7().ToString("N");

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Register_AddsOnce_ReportsOnlyNewSubscriptions()
    {
        var type = NewMessageType();

        var first = await _routes.RegisterAsync([Route(type, Sync, initialize: true), Route(type, Notify)], [], "build-1", Ct);
        var again = await _routes.RegisterAsync([Route(type, Sync), Route(type, Notify)], [], "build-2", Ct);

        first.ShouldBe([Sync, Notify], ignoreOrder: true);
        again.ShouldBeEmpty();
        (await ReadAsync(type, Sync)).ShouldBe((MessageRouteTable.Active, "build-1", true));
        (await DeliveringAsync(type)).ShouldBe([Notify, Sync], ignoreOrder: true);
    }

    [Fact]
    public async Task OldBuildStartsLater_RoutesOfNewBuildKept()
    {
        var type = NewMessageType();
        await _routes.RegisterAsync([Route(type, Sync), Route(type, Audit)], [], "build-2", Ct);

        await _routes.RegisterAsync([Route(type, Sync)], [], "build-1", Ct);

        (await DeliveringAsync(type)).ShouldBe([Audit, Sync], ignoreOrder: true);
    }

    [Fact]
    public async Task Retired_KeepsDelivering_RollbackDoesNotReactivate()
    {
        var type = NewMessageType();
        await _routes.RegisterAsync([Route(type, Sync)], [], "build-1", Ct);

        await _routes.RegisterAsync([], [new RouteRetirement(Sync, "改由新订阅处理")], "build-2", Ct);
        await _routes.RegisterAsync([Route(type, Sync)], [], "build-1", Ct);

        (await ReadAsync(type, Sync)).Status.ShouldBe(MessageRouteTable.Retiring);
        (await DeliveringAsync(type)).ShouldBe([Sync]);
    }

    [Fact]
    public async Task Finalized_DeclaringBuildRefusesToStart()
    {
        var type = NewMessageType();
        await _routes.RegisterAsync([Route(type, Sync)], [], "build-1", Ct);
        await using (var finalize = database.DataSource.CreateCommand(
            $"""UPDATE {MessageRouteTable.Name} SET "Status" = {MessageRouteTable.Finalized} WHERE "MessageType" = @type"""))
        {
            finalize.Parameters.AddWithValue("type", type);
            await finalize.ExecuteNonQueryAsync(Ct);
        }

        var thrown = await Should.ThrowAsync<InvalidOperationException>(() => _routes.RegisterAsync([Route(type, Sync)], [], "build-1", Ct));

        thrown.Message.ShouldContain(Sync);
        (await DeliveringAsync(type)).ShouldBeEmpty();
    }

    private string Sync => "Echo.Sync" + _suffix;

    private string Notify => "Echo.Notify" + _suffix;

    private string Audit => "Echo.Audit" + _suffix;

    private static string NewMessageType() => $"Probe.Event{Guid.CreateVersion7():N}";

    private static RouteDeclaration Route(string type, string subscription, bool initialize = false) =>
        new(new MessageRoute(type, subscription, "Echo"), initialize);

    private async Task<string[]> DeliveringAsync(string type) =>
        [.. (await _routes.ListDeliveringAsync(Ct)).Where(r => string.Equals(r.MessageType, type, StringComparison.Ordinal)).Select(r => r.Subscription)];

    private async Task<(short Status, string Build, bool NeedsInitialization)> ReadAsync(string type, string subscription)
    {
        await using var command = database.DataSource.CreateCommand(
            $"""SELECT "Status", "RegisteredBuild", "NeedsInitialization" FROM {MessageRouteTable.Name} WHERE "MessageType" = @type AND "Subscription" = @subscription""");
        command.Parameters.AddWithValue("type", type);
        command.Parameters.AddWithValue("subscription", subscription);
        await using var reader = await command.ExecuteReaderAsync(Ct);
        (await reader.ReadAsync(Ct)).ShouldBeTrue();
        return (reader.GetInt16(0), reader.GetString(1), reader.GetBoolean(2));
    }
}

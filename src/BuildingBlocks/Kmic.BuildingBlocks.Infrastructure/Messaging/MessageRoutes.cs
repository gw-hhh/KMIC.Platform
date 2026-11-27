using Kmic.BuildingBlocks.Application.Integration;
using Npgsql;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 订阅路由读写（路由只增不删，停投须由运维终结）
internal sealed class MessageRoutes(NpgsqlDataSource dataSource)
{
    public static IReadOnlyList<RouteDeclaration> Declared(SubscriptionCatalog catalog) =>
        [.. catalog.All
            .GroupBy(s => (MessageType: s.Message.Name, Subscription: s.Name))
            .Select(g => new RouteDeclaration(
                new MessageRoute(g.Key.MessageType, g.Key.Subscription, g.First().Module), g.Any(s => s.Rebuild is not null)))];

    public static IReadOnlyList<RouteRetirement> Retirements(SubscriptionCatalog catalog) =>
        [.. catalog.Retired.Select(r => new RouteRetirement(r.Name, r.Reason))];

    // 登记路由并返回首次新增的订阅；声明的订阅已终结时抛出 InvalidOperationException
    public async Task<IReadOnlyList<string>> RegisterAsync(
        IReadOnlyList<RouteDeclaration> declared, IReadOnlyList<RouteRetirement> retired, string build, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(declared);
        ArgumentNullException.ThrowIfNull(retired);

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        // 多个实例同时启动时排队登记
        await using (var command = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtext('Kmic.MessageRoutes'))", connection, transaction))
        {
            await command.ExecuteNonQueryAsync(ct);
        }

        await RefuseFinalizedAsync(connection, transaction, declared, ct);

        var added = new List<string>();
        foreach (var item in declared)
        {
            await using var insert = new NpgsqlCommand(
                $"""
                INSERT INTO {MessageRouteTable.Name} ("MessageType", "Subscription", "Module", "RegisteredBuild", "NeedsInitialization")
                VALUES (@type, @subscription, @module, @build, @initialize)
                ON CONFLICT DO NOTHING
                RETURNING "Subscription"
                """,
                connection,
                transaction);
            insert.Parameters.AddWithValue("type", item.Route.MessageType);
            insert.Parameters.AddWithValue("subscription", item.Route.Subscription);
            insert.Parameters.AddWithValue("module", item.Route.Module);
            insert.Parameters.AddWithValue("build", build);
            insert.Parameters.AddWithValue("initialize", item.NeedsInitialization);
            if (await insert.ExecuteScalarAsync(ct) is string subscription)
            {
                added.Add(subscription);
            }
        }

        // 仅允许从生效状态转为待终结，不可逆转，防止回滚至旧构建时意外重新启用已退出的订阅
        foreach (var item in retired)
        {
            await using var retire = new NpgsqlCommand(
                $"""
                UPDATE {MessageRouteTable.Name} SET "Status" = {MessageRouteTable.Retiring}, "RetiredReason" = @reason
                WHERE "Subscription" = @subscription AND "Status" = {MessageRouteTable.Active}
                """,
                connection,
                transaction);
            retire.Parameters.AddWithValue("subscription", item.Subscription);
            retire.Parameters.AddWithValue("reason", item.Reason);
            await retire.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
        return added;
    }

    public async Task<IReadOnlyList<MessageRoute>> ListDeliveringAsync(CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand(
            $"""
            SELECT "MessageType", "Subscription", "Module" FROM {MessageRouteTable.Name}
            WHERE "Status" IN ({MessageRouteTable.Active}, {MessageRouteTable.Retiring})
            """);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var routes = new List<MessageRoute>();
        while (await reader.ReadAsync(ct))
        {
            routes.Add(new MessageRoute(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        }

        return routes;
    }

    public async Task<IReadOnlyList<string>> ListNeedingInitializationAsync(IReadOnlyCollection<string> subscriptions, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(subscriptions);

        await using var command = dataSource.CreateCommand(
            $"""
            SELECT DISTINCT "Subscription" FROM {MessageRouteTable.Name}
            WHERE "NeedsInitialization" AND "Status" IN ({MessageRouteTable.Active}, {MessageRouteTable.Retiring}) AND "Subscription" = ANY(@subscriptions)
            """);
        command.Parameters.AddWithValue("subscriptions", subscriptions.ToArray());
        await using var reader = await command.ExecuteReaderAsync(ct);
        var pending = new List<string>();
        while (await reader.ReadAsync(ct))
        {
            pending.Add(reader.GetString(0));
        }

        return pending;
    }

    private static async Task RefuseFinalizedAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, IReadOnlyList<RouteDeclaration> declared, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            $"""
            SELECT "Subscription" FROM {MessageRouteTable.Name}
            WHERE "Status" = {MessageRouteTable.Finalized} AND "Subscription" = ANY(@subscriptions)
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("subscriptions", declared.Select(d => d.Route.Subscription).Distinct(StringComparer.Ordinal).ToArray());
        await using var reader = await command.ExecuteReaderAsync(ct);
        var finalized = new List<string>();
        while (await reader.ReadAsync(ct))
        {
            finalized.Add(reader.GetString(0));
        }

        if (finalized.Count > 0)
        {
            throw new InvalidOperationException(
                $"订阅 {string.Join("、", finalized)} 已终结，声明该订阅的构建禁止启动。如需恢复，请联系运维重新激活并核对补投范围。");
        }
    }
}

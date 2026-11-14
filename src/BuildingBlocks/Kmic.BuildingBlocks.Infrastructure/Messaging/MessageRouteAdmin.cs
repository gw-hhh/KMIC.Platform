using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// <see cref="IMessageRouteAdmin"/> 的实现
/// </summary>
/// <remarks>
/// 一个事务里先取登记路由用的同一把咨询锁（和实例启动时的登记、别的运维操作排队），再锁住路由行，
/// 检查之后才改。路由表和订阅方的收件箱在同一个数据库里，所以能放进同一个事务
/// </remarks>
internal sealed class MessageRouteAdmin(DataSources dataSources, ModuleFrameworkTables tables, TimeProvider time) : IMessageRouteAdmin
{
    private static readonly JsonSerializerOptions _probeOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<RouteFinalizeResult> FinalizeAsync(
        string messageType, string subscription, UserId by, string reason, bool abandonUnfinished, TimeSpan instanceAliveWithin, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        await using var connection = await dataSources.Transactions(CallOrigin.Http).OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await LockAsync(connection, transaction, ct);

        var route = await ReadRouteAsync(connection, transaction, messageType, subscription, ct);
        if (route is null)
        {
            return new RouteFinalizeResult(RouteAdminOutcome.NotFound, 0);
        }

        if (route.Status != MessageRouteTable.Retiring)
        {
            return new RouteFinalizeResult(RouteAdminOutcome.WrongStatus, 0);
        }

        if (await IsDeclaredByLiveInstanceAsync(connection, transaction, messageType, subscription, instanceAliveWithin, ct))
        {
            return new RouteFinalizeResult(RouteAdminOutcome.StillDeclared, 0);
        }

        var inbox = tables.For(route.Module).Inbox;
        if (await CountAsync(connection, transaction, inbox, subscription, inFlight: true, ct) > 0)
        {
            return new RouteFinalizeResult(RouteAdminOutcome.InFlight, 0);
        }

        var unfinished = await CountAsync(connection, transaction, inbox, subscription, inFlight: false, ct);
        if (unfinished > 0)
        {
            if (!abandonUnfinished)
            {
                return new RouteFinalizeResult(RouteAdminOutcome.HasUnfinished, unfinished);
            }

            await AbandonUnfinishedAsync(connection, transaction, inbox, subscription, reason, ct);
        }

        await using var update = new NpgsqlCommand(
            $"""
            UPDATE {MessageRouteTable.Name} SET "Status" = {MessageRouteTable.Finalized}, "FinalizedAt" = @now, "FinalizedBy" = @by
            WHERE "MessageType" = @type AND "Subscription" = @subscription
            """,
            connection,
            transaction);
        update.Parameters.AddWithValue("now", time.GetUtcNow());
        update.Parameters.AddWithValue("by", by.Value);
        update.Parameters.AddWithValue("type", messageType);
        update.Parameters.AddWithValue("subscription", subscription);
        await update.ExecuteNonQueryAsync(ct);
        await transaction.CommitAsync(ct);
        return new RouteFinalizeResult(RouteAdminOutcome.Done, unfinished);
    }

    public async Task<RouteAdminOutcome> ReactivateAsync(string messageType, string subscription, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);

        await using var connection = await dataSources.Transactions(CallOrigin.Http).OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await LockAsync(connection, transaction, ct);

        var route = await ReadRouteAsync(connection, transaction, messageType, subscription, ct);
        if (route is null)
        {
            return RouteAdminOutcome.NotFound;
        }

        if (route.Status != MessageRouteTable.Finalized)
        {
            return RouteAdminOutcome.WrongStatus;
        }

        await using var update = new NpgsqlCommand(
            $"""
            UPDATE {MessageRouteTable.Name}
            SET "Status" = {MessageRouteTable.Active}, "FinalizedAt" = NULL, "FinalizedBy" = NULL, "RetiredReason" = NULL
            WHERE "MessageType" = @type AND "Subscription" = @subscription
            """,
            connection,
            transaction);
        update.Parameters.AddWithValue("type", messageType);
        update.Parameters.AddWithValue("subscription", subscription);
        await update.ExecuteNonQueryAsync(ct);
        await transaction.CommitAsync(ct);
        return RouteAdminOutcome.Done;
    }

    public async Task<RedeliverResult> RedeliverAsync(string sourceModule, string messageType, string subscription, long fromSeq, long toSeq, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceModule);
        ArgumentException.ThrowIfNullOrWhiteSpace(messageType);
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);

        if (!tables.Contains(sourceModule))
        {
            return new RedeliverResult(RouteAdminOutcome.UnknownSourceModule, 0, 0);
        }

        await using var connection = await dataSources.Transactions(CallOrigin.Http).OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await LockAsync(connection, transaction, ct);

        var route = await ReadRouteAsync(connection, transaction, messageType, subscription, ct);
        if (route is null)
        {
            return new RedeliverResult(RouteAdminOutcome.NotFound, 0, 0);
        }

        // 已终结的不再投递，补投进去也没人处理，要先重新激活
        if (route.Status == MessageRouteTable.Finalized)
        {
            return new RedeliverResult(RouteAdminOutcome.WrongStatus, 0, 0);
        }

        var ids = await LockSourceRowsAsync(connection, transaction, sourceModule, messageType, fromSeq, toSeq, ct);
        if (ids.Count > IMessageRouteAdmin.MaxRedeliverRows)
        {
            return new RedeliverResult(RouteAdminOutcome.TooManyRows, ids.Count, 0);
        }

        var written = ids.Count == 0 ? 0 : await InsertAsync(connection, transaction, sourceModule, route.Module, subscription, ids, ct);
        if (written > 0)
        {
            await using var notify = new NpgsqlCommand("SELECT pg_notify(@channel, @module)", connection, transaction);
            notify.Parameters.AddWithValue("channel", MessagingChannels.Inbox);
            notify.Parameters.AddWithValue("module", route.Module);
            await notify.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
        return new RedeliverResult(RouteAdminOutcome.Done, ids.Count, written);
    }

    // 先锁住来源行再写入：和正常投递是同一个复核，来源行被清理之后不会再有迟到的补投。只补已经投递过的，
    // 还没投递的、死信不在这里处理。多取一条，用来判断是不是超过了一次的上限
    private async Task<List<Guid>> LockSourceRowsAsync(
        NpgsqlConnection connection, NpgsqlTransaction transaction, string sourceModule, string messageType, long fromSeq, long toSeq, CancellationToken ct)
    {
        var ids = new List<Guid>();
        await using var select = new NpgsqlCommand(
            $"""
            SELECT "Id" FROM {tables.For(sourceModule).Outbox}
            WHERE "MessageType" = @type AND "Seq" BETWEEN @from AND @to AND "Status" = {MessageStatus.Done}
            ORDER BY "Seq" LIMIT @limit
            FOR SHARE
            """,
            connection,
            transaction);
        select.Parameters.AddWithValue("type", messageType);
        select.Parameters.AddWithValue("from", fromSeq);
        select.Parameters.AddWithValue("to", toSeq);
        select.Parameters.AddWithValue("limit", IMessageRouteAdmin.MaxRedeliverRows + 1);
        await using var reader = await select.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            ids.Add(reader.GetGuid(0));
        }

        return ids;
    }

    // 和正常投递写同样的列，序号沿用来源的序号；订阅已经收到过的（同一条消息同一个订阅）由唯一约束挡住
    private async Task<long> InsertAsync(
        NpgsqlConnection connection, NpgsqlTransaction transaction, string sourceModule, string targetModule, string subscription, List<Guid> ids, CancellationToken ct)
    {
        await using var insert = new NpgsqlCommand(
            $"""
            INSERT INTO {tables.For(targetModule).Inbox} ("Id", "MessageId", "Subscription", "MessageType", "SchemaVersion", "Payload", "Headers",
                "SourceModule", "SourceId", "SourceVersion", "OrderingKey", "Seq", "SubjectIds", "OccurredAt", "Status", "Attempts")
            SELECT uuidv7(), o."Id", @subscription, o."MessageType", o."SchemaVersion", o."Payload", o."Headers",
                @source, o."SourceId", o."SourceVersion", o."OrderingKey", o."Seq", o."SubjectIds", o."OccurredAt", {MessageStatus.Pending}, 0
            FROM {tables.For(sourceModule).Outbox} o
            WHERE o."Id" = ANY(@ids)
            ORDER BY o."Seq"
            ON CONFLICT ("MessageId", "Subscription") DO NOTHING
            """,
            connection,
            transaction);
        insert.Parameters.AddWithValue("subscription", subscription);
        insert.Parameters.AddWithValue("source", sourceModule);
        insert.Parameters.AddWithValue("ids", ids.ToArray());
        return await insert.ExecuteNonQueryAsync(ct);
    }

    private static async Task LockAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtext('Kmic.MessageRoutes'))", connection, transaction);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static async Task<RouteState?> ReadRouteAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string messageType, string subscription, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            $"""SELECT "Status", "Module" FROM {MessageRouteTable.Name} WHERE "MessageType" = @type AND "Subscription" = @subscription FOR UPDATE""",
            connection,
            transaction);
        command.Parameters.AddWithValue("type", messageType);
        command.Parameters.AddWithValue("subscription", subscription);
        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? new RouteState(reader.GetInt16(0), reader.GetString(1)) : null;
    }

    // 实例登记的订阅清单里写着消息名称和订阅名，用包含运算匹配；超过时限没有心跳的实例不算
    private async Task<bool> IsDeclaredByLiveInstanceAsync(
        NpgsqlConnection connection, NpgsqlTransaction transaction, string messageType, string subscription, TimeSpan aliveWithin, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            $"""
            SELECT EXISTS (SELECT 1 FROM {MessagingInstanceTable.Name} WHERE "HeartbeatAt" > @since AND "Subscriptions" @> @probe)
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("since", time.GetUtcNow() - aliveWithin);
        command.Parameters.Add(new NpgsqlParameter("probe", NpgsqlDbType.Jsonb) { Value = JsonSerializer.Serialize(new[] { new { messageType, subscription } }, _probeOptions) });
        return (bool)(await command.ExecuteScalarAsync(ct))!;
    }

    // 正在处理且占用还没到期的，或者没处理完的：待处理、死信，以及占用已经到期的处理中
    private static async Task<long> CountAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string inbox, string subscription, bool inFlight, CancellationToken ct)
    {
        var condition = inFlight
            ? $"\"Status\" = {MessageStatus.Processing} AND \"LeaseUntil\" > clock_timestamp()"
            : $"\"Status\" IN ({MessageStatus.Pending}, {MessageStatus.Dead}) OR (\"Status\" = {MessageStatus.Processing} AND \"LeaseUntil\" <= clock_timestamp())";
        await using var command = new NpgsqlCommand($"""SELECT count(*) FROM {inbox} WHERE "Subscription" = @subscription AND ({condition})""", connection, transaction);
        command.Parameters.AddWithValue("subscription", subscription);
        return (long)(await command.ExecuteScalarAsync(ct))!;
    }

    private static async Task AbandonUnfinishedAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string inbox, string subscription, string reason, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            $"""
            UPDATE {inbox}
            SET "Status" = {MessageStatus.Abandoned}, "LeaseToken" = NULL, "LeaseUntil" = NULL,
                "LastError" = left('终结订阅时放弃：' || @reason || '；此前的错误：' || COALESCE("LastError", '无'), 4000)
            WHERE "Subscription" = @subscription
              AND ("Status" IN ({MessageStatus.Pending}, {MessageStatus.Dead}) OR ("Status" = {MessageStatus.Processing} AND "LeaseUntil" <= clock_timestamp()))
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("subscription", subscription);
        command.Parameters.AddWithValue("reason", reason);
        await command.ExecuteNonQueryAsync(ct);
    }

    private sealed record RouteState(short Status, string Module);
}

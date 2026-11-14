using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Infrastructure.Jobs;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 重建轮次的读写
/// </summary>
/// <remarks>
/// 轮次和它的作业行在订阅方模块的 schema 里，排队时同一个事务写入
/// </remarks>
internal sealed class RebuildRunStore(ModuleFrameworkTables tables, DataSources dataSources, BackgroundJobCatalog jobs)
{
    /// <summary>
    /// 给订阅排一轮重建，已经有没结束的一轮时返回空
    /// </summary>
    public async Task<Guid?> QueueAsync(CallOrigin origin, string module, string subscription, string readModel, CancellationToken ct)
    {
        var names = tables.For(module);
        await using var connection = await dataSources.Transactions(origin).OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        await CloseOrphanedRunAsync(connection, transaction, names, subscription, ct);

        var runId = Guid.CreateVersion7();
        var jobId = Guid.CreateVersion7();
        if (!await InsertRunAsync(connection, transaction, names, (runId, jobId, subscription, readModel), ct))
        {
            return null;
        }

        await InsertJobAsync(connection, transaction, names, jobId, new ReadModelRebuildJob(subscription, runId), ct);
        await NotifyJobsAsync(connection, transaction, module, ct);
        await transaction.CommitAsync(ct);
        return runId;
    }

    // 作业已经结束（失败、取消、被清理）而轮次还挂着的，先收尾，免得它一直占着唯一索引
    private static async Task CloseOrphanedRunAsync(
        NpgsqlConnection connection, NpgsqlTransaction transaction, FrameworkTableNames names, string subscription, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            $"""
            UPDATE {names.RebuildRuns} r SET "Status" = {RebuildRunStatus.Failed}, "FinishedAt" = clock_timestamp()
            WHERE r."Subscription" = @subscription AND r."Status" = {RebuildRunStatus.Active}
              AND NOT EXISTS (
                  SELECT 1 FROM {names.Jobs} j
                  WHERE j."Id" = r."JobId" AND j."Status" IN ({JobStatus.Queued}, {JobStatus.Running}, {JobStatus.Cancelling}))
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("subscription", subscription);
        await command.ExecuteNonQueryAsync(ct);
    }

    // 同一个订阅已经有没结束的一轮时，唯一索引让这条插入什么都不做
    private static async Task<bool> InsertRunAsync(
        NpgsqlConnection connection, NpgsqlTransaction transaction, FrameworkTableNames names, (Guid RunId, Guid JobId, string Subscription, string ReadModel) run, CancellationToken ct)
    {
        await using var command = new NpgsqlCommand(
            $"""
            INSERT INTO {names.RebuildRuns} ("Id", "Subscription", "ReadModel", "JobId", "Status", "ScannedCount", "MissingCount")
            VALUES (@id, @subscription, @readModel, @jobId, {RebuildRunStatus.Active}, 0, 0)
            ON CONFLICT ("Subscription") WHERE "Status" IN ({RebuildRunStatus.Active}, {RebuildRunStatus.AwaitingConfirmation}) DO NOTHING
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("id", run.RunId);
        command.Parameters.AddWithValue("subscription", run.Subscription);
        command.Parameters.AddWithValue("readModel", run.ReadModel);
        command.Parameters.AddWithValue("jobId", run.JobId);
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    private async Task InsertJobAsync(
        NpgsqlConnection connection, NpgsqlTransaction transaction, FrameworkTableNames names, Guid jobId, ReadModelRebuildJob job, CancellationToken ct)
    {
        var type = jobs.Describe(typeof(ReadModelRebuildJob));
        await using var command = new NpgsqlCommand(
            $"""
            INSERT INTO {names.Jobs} ("Id", "JobType", "SchemaVersion", "Args", "SubmittedAt", "Status", "ProgressDone", "Attempts")
            VALUES (@id, @type, @version, @args, clock_timestamp(), {JobStatus.Queued}, 0, 0)
            """,
            connection,
            transaction);
        command.Parameters.AddWithValue("id", jobId);
        command.Parameters.AddWithValue("type", type.Name);
        command.Parameters.AddWithValue("version", type.Version);
        command.Parameters.Add(new NpgsqlParameter("args", NpgsqlDbType.Jsonb)
        {
            Value = JsonSerializer.Serialize(job, KmicJsonOptions.Stored),
        });
        await command.ExecuteNonQueryAsync(ct);
    }


    /// <summary>
    /// 这一轮完成：记下数量，并把订阅的“需要初始化”标记清掉
    /// </summary>
    public async Task CompleteAsync(string module, string subscription, Guid runId, long scanned, long missing, CancellationToken ct)
    {
        await using var connection = await dataSources.Transactions(CallOrigin.BackgroundJob).OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        await using (var complete = new NpgsqlCommand(
            $"""
            UPDATE {tables.For(module).RebuildRuns}
            SET "Status" = {RebuildRunStatus.Completed}, "ScannedCount" = @scanned, "MissingCount" = @missing, "FinishedAt" = clock_timestamp()
            WHERE "Id" = @id AND "Status" = {RebuildRunStatus.Active}
            """,
            connection,
            transaction))
        {
            AddCounts(complete, runId, scanned, missing);

            // 这一轮已经被运维放弃或标记失败时不再改订阅，标记留给下一轮
            if (await complete.ExecuteNonQueryAsync(ct) == 0)
            {
                return;
            }
        }

        await using (var initialized = new NpgsqlCommand(
            $"""
            UPDATE {MessageRouteTable.Name} SET "NeedsInitialization" = false WHERE "Subscription" = @subscription
            """,
            connection,
            transaction))
        {
            initialized.Parameters.AddWithValue("subscription", subscription);
            await initialized.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }

    /// <summary>
    /// 源端找不到的行太多，暂停等运维确认
    /// </summary>
    public async Task PauseAsync(string module, Guid runId, long scanned, long missing, CancellationToken ct)
    {
        await using var command = dataSources.Transactions(CallOrigin.BackgroundJob).CreateCommand(
            $"""
            UPDATE {tables.For(module).RebuildRuns}
            SET "Status" = {RebuildRunStatus.AwaitingConfirmation}, "ScannedCount" = @scanned, "MissingCount" = @missing
            WHERE "Id" = @id AND "Status" = {RebuildRunStatus.Active}
            """);
        AddCounts(command, runId, scanned, missing);
        await command.ExecuteNonQueryAsync(ct);
    }

    // 通知在提交时才送达，执行器被唤醒时一定读得到作业行
    private static async Task NotifyJobsAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string module, CancellationToken ct)
    {
        await using var notify = new NpgsqlCommand("SELECT pg_notify(@channel, @module)", connection, transaction);
        notify.Parameters.AddWithValue("channel", MessagingChannels.Jobs);
        notify.Parameters.AddWithValue("module", module);
        await notify.ExecuteNonQueryAsync(ct);
    }

    /// <summary>
    /// 运维确认收尾：暂停等确认的一轮回到进行中，另排一个只做收尾的作业。没有暂停的一轮时返回 false
    /// </summary>
    public async Task<bool> ConfirmAsync(string module, string subscription, CancellationToken ct)
    {
        var names = tables.For(module);
        await using var connection = await dataSources.Transactions(CallOrigin.Http).OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        Guid runId;
        long scanned;
        var jobId = Guid.CreateVersion7();
        await using (var resume = new NpgsqlCommand(
            $"""
            UPDATE {names.RebuildRuns}
            SET "Status" = {RebuildRunStatus.Active}, "JobId" = @jobId
            WHERE "Subscription" = @subscription AND "Status" = {RebuildRunStatus.AwaitingConfirmation}
            RETURNING "Id", "ScannedCount"
            """,
            connection,
            transaction))
        {
            resume.Parameters.AddWithValue("jobId", jobId);
            resume.Parameters.AddWithValue("subscription", subscription);
            await using var reader = await resume.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return false;
            }

            runId = reader.GetGuid(0);
            scanned = reader.GetInt64(1);
        }

        await InsertJobAsync(connection, transaction, names, jobId, new ReadModelRebuildJob(subscription, runId, ConfirmMissing: true, Scanned: scanned), ct);
        await NotifyJobsAsync(connection, transaction, module, ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    /// <summary>
    /// 运维放弃暂停的一轮，同时清除订阅的“需要初始化”标记，此后不会自动重建。没有暂停的一轮时返回 false
    /// </summary>
    public async Task<bool> AbandonAsync(string module, string subscription, CancellationToken ct)
    {
        await using var connection = await dataSources.Transactions(CallOrigin.Http).OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        await using (var abandon = new NpgsqlCommand(
            $"""
            UPDATE {tables.For(module).RebuildRuns}
            SET "Status" = {RebuildRunStatus.Abandoned}, "FinishedAt" = clock_timestamp()
            WHERE "Subscription" = @subscription AND "Status" = {RebuildRunStatus.AwaitingConfirmation}
            """,
            connection,
            transaction))
        {
            abandon.Parameters.AddWithValue("subscription", subscription);
            if (await abandon.ExecuteNonQueryAsync(ct) == 0)
            {
                return false;
            }
        }

        await using (var initialized = new NpgsqlCommand($"""UPDATE {MessageRouteTable.Name} SET "NeedsInitialization" = false WHERE "Subscription" = @subscription""", connection, transaction))
        {
            initialized.Parameters.AddWithValue("subscription", subscription);
            await initialized.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
        return true;
    }

    /// <summary>
    /// 一个模块最近的重建轮次，新的在前
    /// </summary>
    public async Task<IReadOnlyList<RebuildRunInfo>> ListAsync(string module, int take, CancellationToken ct)
    {
        await using var command = dataSources.Reads(CallOrigin.Http).CreateCommand(
            $"""
            SELECT "Id", "Subscription", "ReadModel", "JobId", "Status", "ScannedCount", "MissingCount", "StartedAt", "FinishedAt"
            FROM {tables.For(module).RebuildRuns} ORDER BY "StartedAt" DESC, "Id" DESC LIMIT @take
            """);
        command.Parameters.AddWithValue("take", take);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var runs = new List<RebuildRunInfo>();
        while (await reader.ReadAsync(ct))
        {
            runs.Add(new RebuildRunInfo(
                module,
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetGuid(3),
                StateOf(reader.GetInt16(4)),
                reader.GetInt64(5),
                reader.GetInt64(6),
                await reader.GetFieldValueAsync<DateTimeOffset>(7, ct),
                await reader.IsDBNullAsync(8, ct) ? null : await reader.GetFieldValueAsync<DateTimeOffset>(8, ct)));
        }

        return runs;
    }

    private static RebuildRunState StateOf(short status) =>
        status switch
        {
            RebuildRunStatus.Active => RebuildRunState.Active,
            RebuildRunStatus.AwaitingConfirmation => RebuildRunState.AwaitingConfirmation,
            RebuildRunStatus.Completed => RebuildRunState.Completed,
            RebuildRunStatus.Abandoned => RebuildRunState.Abandoned,
            _ => RebuildRunState.Failed,
        };

    private static void AddCounts(NpgsqlCommand command, Guid runId, long scanned, long missing)
    {
        command.Parameters.AddWithValue("id", runId);
        command.Parameters.AddWithValue("scanned", scanned);
        command.Parameters.AddWithValue("missing", missing);
    }
}

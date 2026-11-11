using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// 作业行的读写
/// </summary>
/// <remarks>
/// 凡是记录结果的写入（完成、失败、放回队列、进度、检查点）都带“运行中或取消中、令牌是本次的”条件：
/// 占用被接管的旧执行者恢复后，改不了新执行者正在跑的作业，也不会重复写结果
/// </remarks>
internal sealed class JobStore(ModuleFrameworkTables tables, DataSources dataSources)
{
    private const string Owned = """
        "Id" = @id AND "LeaseToken" = @token AND "Status" IN (1, 4)
        """;

    private NpgsqlDataSource Database => dataSources.Transactions(CallOrigin.BackgroundJob);

    public async Task<IReadOnlyList<ClaimedJob>> ClaimAsync(string module, int max, Guid token, TimeSpan lease, CancellationToken ct)
    {
        var jobs = tables.For(module).Jobs;

        // 排队的按提交顺序领；运行中或取消中但占用已到期的，说明执行者失联了，也领回来处理
        await using var command = Database.CreateCommand(
            $"""
            WITH c AS (
                SELECT "Id", "Status" AS "Previous" FROM {jobs}
                WHERE "Status" = {JobStatus.Queued}
                   OR ("Status" IN ({JobStatus.Running}, {JobStatus.Cancelling}) AND "LeaseUntil" < clock_timestamp())
                ORDER BY "SubmittedAt"
                LIMIT @max
                FOR UPDATE SKIP LOCKED)
            UPDATE {jobs} j
            SET "Status" = {JobStatus.Running}, "LeaseToken" = @token, "LeaseUntil" = clock_timestamp() + @lease,
                "Attempts" = j."Attempts" + 1, "StartedAt" = COALESCE(j."StartedAt", clock_timestamp())
            FROM c WHERE j."Id" = c."Id"
            RETURNING j."Id", j."JobType", j."SchemaVersion", j."Args"::text, j."SubmittedBy", j."SubmittedRoleId",
                c."Previous", j."Attempts", j."Checkpoint"::text
            """);
        command.Parameters.AddWithValue("max", max);
        command.Parameters.AddWithValue("token", token);
        command.Parameters.AddWithValue("lease", lease);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var claimed = new List<ClaimedJob>();
        while (await reader.ReadAsync(ct))
        {
            claimed.Add(new ClaimedJob
            {
                Id = reader.GetGuid(0),
                JobType = reader.GetString(1),
                SchemaVersion = reader.GetInt32(2),
                Args = reader.GetString(3),
                SubmittedBy = await reader.GetFieldValueAsync<Guid?>(4, ct),
                SubmittedRoleId = await reader.GetFieldValueAsync<Guid?>(5, ct),
                Previous = reader.GetInt16(6),
                Attempts = reader.GetInt32(7),
                Checkpoint = await reader.IsDBNullAsync(8, ct) ? null : reader.GetString(8),
            });
        }

        return claimed;
    }

    /// <summary>
    /// 续期，返回续期后的状态和已经运行的时长；占用已被接管时返回空
    /// </summary>
    /// <remarks>
    /// 运行时长按数据库时钟从第一次开始算起，续跑也不重新计时
    /// </remarks>
    public async Task<(short Status, TimeSpan Elapsed)?> RenewAsync(string module, Guid id, Guid token, TimeSpan lease, CancellationToken ct)
    {
        await using var command = Database.CreateCommand(
            $"""
            UPDATE {tables.For(module).Jobs} SET "LeaseUntil" = clock_timestamp() + @lease WHERE {Owned}
            RETURNING "Status", clock_timestamp() - "StartedAt"
            """);
        command.Parameters.AddWithValue("lease", lease);
        AddOwner(command, id, token);
        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? (reader.GetInt16(0), await reader.GetFieldValueAsync<TimeSpan>(1, ct)) : null;
    }

    public async Task<bool> ReportProgressAsync(string module, Guid id, Guid token, long done, long? total, string? message, CancellationToken ct)
    {
        await using var command = Database.CreateCommand(
            $"""
            UPDATE {tables.For(module).Jobs}
            SET "ProgressDone" = @done, "ProgressTotal" = COALESCE(@total, "ProgressTotal"), "ProgressMessage" = COALESCE(@message, "ProgressMessage")
            WHERE {Owned}
            """);
        command.Parameters.AddWithValue("done", done);
        command.Parameters.Add(new NpgsqlParameter("total", NpgsqlDbType.Bigint) { Value = (object?)total ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("message", NpgsqlDbType.Varchar) { Value = (object?)Truncate(message, 500) ?? DBNull.Value });
        AddOwner(command, id, token);
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    /// <summary>
    /// 在批次命令的事务里写检查点和进度，和业务数据一起提交
    /// </summary>
    public async Task<bool> SaveCheckpointAsync(ModuleDbContext db, string module, Guid id, Guid token, string checkpoint, long done, CancellationToken ct)
    {
        var sql = $"""
            UPDATE {tables.For(module).Jobs} SET "Checkpoint" = @checkpoint, "ProgressDone" = @done WHERE {Owned}
            """;
        return await db.Database.ExecuteSqlRawAsync(
            sql,
            [new NpgsqlParameter("checkpoint", NpgsqlDbType.Jsonb) { Value = checkpoint }, new NpgsqlParameter("done", done),
             new NpgsqlParameter("id", id), new NpgsqlParameter("token", token)],
            ct) == 1;
    }

    public async Task<bool> FinishAsync(
        string module, Guid id, Guid token, short status, Error? error, Guid? resultFileId, string? summary, TimeSpan retention, CancellationToken ct)
    {
        await using var command = Database.CreateCommand(
            $"""
            UPDATE {tables.For(module).Jobs}
            SET "Status" = @status, "ErrorCode" = @code, "ErrorMessage" = @message, "ResultFileId" = @file, "ResultSummary" = @summary,
                "FinishedAt" = clock_timestamp(), "ExpiresAt" = clock_timestamp() + @retention, "LeaseUntil" = NULL
            WHERE {Owned}
            """);
        command.Parameters.AddWithValue("status", status);
        command.Parameters.Add(new NpgsqlParameter("code", NpgsqlDbType.Varchar) { Value = (object?)error?.Code ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("message", NpgsqlDbType.Varchar) { Value = (object?)Truncate(error?.Message, 2000) ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("file", NpgsqlDbType.Uuid) { Value = (object?)resultFileId ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("summary", NpgsqlDbType.Jsonb) { Value = (object?)summary ?? DBNull.Value });
        command.Parameters.AddWithValue("retention", retention);
        AddOwner(command, id, token);
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    /// <summary>
    /// 停机时把可续跑的作业放回队列；这期间用户要求了取消的，直接标为已取消
    /// </summary>
    public async Task<bool> ReleaseAsync(string module, Guid id, Guid token, CancellationToken ct)
    {
        await using var command = Database.CreateCommand(
            $"""
            UPDATE {tables.For(module).Jobs}
            SET "Status" = CASE WHEN "Status" = {JobStatus.Cancelling} THEN {JobStatus.Cancelled} ELSE {JobStatus.Queued} END,
                "LeaseToken" = NULL, "LeaseUntil" = NULL
            WHERE {Owned}
            """);
        AddOwner(command, id, token);
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    /// <summary>
    /// 请求取消：排队中的直接取消，运行中的改为取消中，执行器在下次续期时停下
    /// </summary>
    public async Task<bool> RequestCancelAsync(string module, Guid id, Guid? by, TimeSpan retention, CancellationToken ct)
    {
        await using var command = Database.CreateCommand(
            $"""
            UPDATE {tables.For(module).Jobs}
            SET "Status" = CASE WHEN "Status" = {JobStatus.Queued} THEN {JobStatus.Cancelled} ELSE {JobStatus.Cancelling} END,
                "CancelRequestedAt" = clock_timestamp(), "CancelRequestedBy" = @by,
                "FinishedAt" = CASE WHEN "Status" = {JobStatus.Queued} THEN clock_timestamp() END,
                "ExpiresAt" = CASE WHEN "Status" = {JobStatus.Queued} THEN clock_timestamp() + @retention END
            WHERE "Id" = @id AND "Status" IN ({JobStatus.Queued}, {JobStatus.Running})
            """);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.Add(new NpgsqlParameter("by", NpgsqlDbType.Uuid) { Value = (object?)by ?? DBNull.Value });
        command.Parameters.AddWithValue("retention", retention);
        return await command.ExecuteNonQueryAsync(ct) == 1;
    }

    private static void AddOwner(NpgsqlCommand command, Guid id, Guid token)
    {
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("token", token);
    }

    private static string? Truncate(string? value, int length) => value is { Length: > 0 } && value.Length > length ? value[..length] : value;
}

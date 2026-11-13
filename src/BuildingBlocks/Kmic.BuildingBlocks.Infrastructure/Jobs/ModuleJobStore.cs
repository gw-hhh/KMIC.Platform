using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Npgsql;
using NpgsqlTypes;

namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// <see cref="IModuleJobStore" /> 的实现：读一个模块的作业表，取消走执行器
/// </summary>
/// <remarks>
/// 只选状态相关的列，不读作业参数与检查点。查询走读池：管理视图允许几百毫秒的复制延迟，不占事务池
/// </remarks>
internal sealed class ModuleJobStore(string module, ModuleFrameworkTables tables, DataSources dataSources, JobExecutor executor) : IModuleJobStore
{
    private const string Columns = """
        "Id", "JobType", "Status", "SubmittedBy", "ActualUserId", "SubmittedAt", "StartedAt", "FinishedAt", "ProgressDone", "ProgressTotal",
        "ProgressMessage", "ResultFileId", "ResultSummary"::text AS "ResultSummary", "ErrorCode", "ErrorMessage", "Attempts", "CancelRequestedAt", "CancelRequestedBy"
        """;

    private NpgsqlDataSource Database => dataSources.Reads(CallOrigin.Http);

    public string Module => module;

    public async Task<IReadOnlyList<JobView>> ListAsync(JobFilter filter, JobPosition? after, int take, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentOutOfRangeException.ThrowIfLessThan(take, 1);

        await using var command = Database.CreateCommand(
            $"""
            SELECT {Columns}
            FROM {tables.For(module).Jobs}
            WHERE (@type::text IS NULL OR "JobType" = @type)
              AND (@status::smallint IS NULL OR "Status" = @status)
              AND (@by::uuid IS NULL OR "SubmittedBy" = @by)
              AND (@afterAt::timestamptz IS NULL OR ("SubmittedAt", "Id") < (@afterAt, @afterId))
            ORDER BY "SubmittedAt" DESC, "Id" DESC
            LIMIT @take
            """);
        command.Parameters.Add(new NpgsqlParameter("type", NpgsqlDbType.Text) { Value = (object?)filter.JobType ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("status", NpgsqlDbType.Smallint) { Value = filter.State is { } state ? (short)state : DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("by", NpgsqlDbType.Uuid) { Value = (object?)filter.SubmittedBy ?? DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("afterAt", NpgsqlDbType.TimestampTz) { Value = after is { } position ? position.SubmittedAt : DBNull.Value });
        command.Parameters.Add(new NpgsqlParameter("afterId", NpgsqlDbType.Uuid) { Value = after is { } last ? last.Id : Guid.Empty });
        command.Parameters.AddWithValue("take", take);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var views = new List<JobView>();
        while (await reader.ReadAsync(ct))
        {
            views.Add(Read(reader));
        }

        return views;
    }

    public async Task<JobView?> FindAsync(Guid id, CancellationToken ct)
    {
        await using var command = Database.CreateCommand($"""SELECT {Columns} FROM {tables.For(module).Jobs} WHERE "Id" = @id""");
        command.Parameters.AddWithValue("id", id);
        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    public Task<bool> RequestCancelAsync(Guid id, UserId by, CancellationToken ct) => executor.RequestCancelAsync(module, id, by, ct);

    private JobView Read(NpgsqlDataReader reader) =>
        new(
            reader.GetGuid(0),
            module,
            reader.GetString(1),
            (BackgroundJobState)reader.GetInt16(2),
            reader.IsDBNull(3) ? null : reader.GetGuid(3),
            reader.IsDBNull(4) ? null : reader.GetGuid(4),
            reader.GetFieldValue<DateTimeOffset>(5),
            reader.IsDBNull(6) ? null : reader.GetFieldValue<DateTimeOffset>(6),
            reader.IsDBNull(7) ? null : reader.GetFieldValue<DateTimeOffset>(7),
            reader.GetInt64(8),
            reader.IsDBNull(9) ? null : reader.GetInt64(9),
            reader.IsDBNull(10) ? null : reader.GetString(10),
            reader.IsDBNull(11) ? null : reader.GetGuid(11),
            reader.IsDBNull(12) ? null : reader.GetString(12),
            reader.IsDBNull(13) ? null : reader.GetString(13),
            reader.IsDBNull(14) ? null : reader.GetString(14),
            reader.GetInt32(15),
            reader.IsDBNull(16) ? null : reader.GetFieldValue<DateTimeOffset>(16),
            reader.IsDBNull(17) ? null : reader.GetGuid(17));
}

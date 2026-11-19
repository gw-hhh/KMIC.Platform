using System.Globalization;
using System.Text;
using Kmic.BuildingBlocks.Application.Exports;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Identity.Contracts;
using Microsoft.Extensions.Options;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 把审计记录流式导出为 CSV 文件
/// </summary>
/// <remarks>
/// 文件登记请求和检查点在最后一批的事务里提交
/// </remarks>
public sealed class ExportAuditJobHandler(
    IExportReader<IOperationsQueryDb> reader,
    IGeneratedFiles generatedFiles,
    IUserReferences users,
    IOptions<AuditExportOptions> options,
    TimeProvider time) : IJobHandler<ExportAuditJob>
{
    /// <summary>
    /// 生成文件的来源标识
    /// </summary>
    public const string Source = "Operations.AuditExport";

    /// <inheritdoc />
    public async Task<Result<JobOutcome>> ExecuteAsync(ExportAuditJob job, IJobContext context, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);
        ArgumentNullException.ThrowIfNull(context);

        var path = Path.Combine(Path.GetTempPath(), $"kmic-audit-export-{context.JobId:N}.csv");
        try
        {
            var rows = await WriteCsvAsync(job, path, ct);
            if (rows.IsFailure)
            {
                return rows.Error;
            }

            await context.ReportProgressAsync(rows.Value, rows.Value, "正在保存文件");
            return await SaveAsync(context, path, rows.Value, ct);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private async Task<Result<long>> WriteCsvAsync(ExportAuditJob job, string path, CancellationToken ct)
    {
        var names = new Dictionary<Guid, string?>();
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);

        // 写入 UTF-8 BOM，便于 Excel 识别中文
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        await writer.WriteLineAsync(AuditCsv.Header.AsMemory(), ct);
        var rows = await reader.ReadAsync(
            db => AuditFilters.Apply(db, job.From, job.To, job.ActorUserId, job.ActualUserId, job.Module, job.Action, job.ResourceType, job.ResourceId)
                .OrderBy(a => a.OccurredAt)
                .ThenBy(a => a.Id)
                .Select(a => new AuditExportRow(a.OccurredAt, a.Module, a.Action, a.Origin, a.ActorUserId, a.ActualUserId, a.TraceId, a.Targets)),
            async (row, token) => await writer.WriteLineAsync(
                AuditCsv.Line(
                    row.OccurredAt,
                    row.Module,
                    row.Action,
                    row.Origin,
                    row.ActorUserId?.Value,
                    await NameOfAsync(names, row.ActorUserId, token),
                    row.ActualUserId?.Value,
                    await NameOfAsync(names, row.ActualUserId, token),
                    row.TraceId,
                    row.Targets).AsMemory(),
                token),
            ct);
        await writer.FlushAsync(ct);
        return rows;
    }

    private async Task<Result<JobOutcome>> SaveAsync(IJobContext context, string path, long rows, CancellationToken ct)
    {
        var fileName = string.Create(CultureInfo.InvariantCulture, $"审计导出-{time.GetUtcNow():yyyyMMdd-HHmmss}.csv");
        await using var content = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        var written = await generatedFiles.WriteAsync(Source, fileName, content, options.Value.ExportRetention, ct);
        if (written.IsFailure)
        {
            return written.Error;
        }

        var registered = await context.SendBatchAsync(new RegisterAuditExportFileCommand(written.Value), new AuditExportCheckpoint(Registered: true), rows);
        return registered.IsFailure
            ? registered.Error
            : new JobOutcome(written.Value.FileId, new AuditExportSummary(rows, written.Value.Name));
    }

    // 同一个人在导出里会出现很多次，每人只查一次
    private async Task<string?> NameOfAsync(Dictionary<Guid, string?> cache, UserId? userId, CancellationToken ct)
    {
        if (userId is not { } user)
        {
            return null;
        }

        if (!cache.TryGetValue(user.Value, out var name))
        {
            var found = await users.ResolveAsync([user.Value], ct);
            name = found.GetValueOrDefault(user.Value)?.DisplayName;
            cache[user.Value] = name;
        }

        return name;
    }

    private sealed record AuditExportRow(
        DateTimeOffset OccurredAt,
        string Module,
        string Action,
        string Origin,
        UserId? ActorUserId,
        UserId? ActualUserId,
        string TraceId,
        string Targets);
}

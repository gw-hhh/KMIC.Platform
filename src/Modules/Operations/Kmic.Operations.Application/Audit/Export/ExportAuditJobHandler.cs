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
/// 处理 <see cref="ExportAuditJob"/>：把审计记录逐行写进一个临时 CSV，再交给 Files 保存
/// </summary>
/// <remarks>
/// 读数据走框架的导出入口（流式，不把全部数据装进内存）；文件写进存储之后，由最后发的那个命令请求登记，
/// 登记请求和命令同一个事务进发件箱。临时文件不管成败都删除
/// </remarks>
public sealed class ExportAuditJobHandler(
    IExportReader<IOperationsQueryDb> reader,
    IGeneratedFiles generatedFiles,
    IUserReferences users,
    IOptions<AuditExportOptions> options,
    TimeProvider time) : IJobHandler<ExportAuditJob>
{
    /// <summary>
    /// 生成方名称，写进文件的上传档案
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

        // 带 BOM 的 UTF-8：Excel 才会把中文当成 UTF-8 打开
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

        // 登记请求放进最后这个命令里，和命令同一个事务进发件箱：作业中途失败就不会登记，写进存储的对象由对账清理
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

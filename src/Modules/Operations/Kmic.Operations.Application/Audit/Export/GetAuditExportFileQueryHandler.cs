using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Operations.Contracts;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Audit.Export;

/// <summary>
/// 获取审计导出文件的下载信息
/// </summary>
public sealed class GetAuditExportFileQueryHandler(IModuleJobStores stores, IFileDownloads downloads, IExecutionContext execution, TimeProvider time)
    : IQueryHandler<GetAuditExportFileQuery, FileDownload>
{
    // 文件登记请求一般几秒内处理完；超过这个时间还查不到，按文件不存在返回
    private static readonly TimeSpan _registrationWindow = TimeSpan.FromMinutes(5);

    /// <inheritdoc />
    public async ValueTask<Result<FileDownload>> Handle(GetAuditExportFileQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var store = stores.Find(OperationsModuleInfo.Name);
        var job = store is null ? null : await store.FindAsync(query.JobId, cancellationToken);
        if (job is null || !string.Equals(job.JobType, ExportAuditJob.Name, StringComparison.Ordinal) || !CanSee(job))
        {
            return OperationsErrors.AuditExportNotFound;
        }

        if (job.State is BackgroundJobState.Failed or BackgroundJobState.Cancelled)
        {
            return OperationsErrors.AuditExportFailed;
        }

        if (job.State != BackgroundJobState.Succeeded || job.ResultFileId is not { } fileId)
        {
            return OperationsErrors.AuditExportNotReady;
        }

        // 作业完成时只发出了登记请求，Files 处理完之前查不到这个文件
        var download = await downloads.GetAsync(fileId, cancellationToken);
        return download.IsFailure && download.Error.Type == ErrorType.NotFound && time.GetUtcNow() - job.FinishedAt < _registrationWindow
            ? OperationsErrors.AuditExportNotReady
            : download;
    }

    // 提交导出的人，或者有管理作业权限的人
    private bool CanSee(JobView job) =>
        (execution.User.UserId is { } user && job.SubmittedBy == user.Value)
        || execution.Access?.Permissions.Contains(OperationsPermissions.JobsManage) == true;
}

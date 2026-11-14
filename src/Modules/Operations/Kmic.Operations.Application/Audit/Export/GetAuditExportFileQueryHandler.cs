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
/// 处理 <see cref="GetAuditExportFileQuery"/>
/// </summary>
public sealed class GetAuditExportFileQueryHandler(IModuleJobStores stores, IFileDownloads downloads, IExecutionContext execution)
    : IQueryHandler<GetAuditExportFileQuery, FileDownload>
{
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

        return await downloads.GetAsync(fileId, cancellationToken);
    }

    // 提交导出的人，或者有管理作业权限的人
    private bool CanSee(JobView job) =>
        (execution.User.UserId is { } user && job.SubmittedBy == user.Value)
        || execution.Access?.Permissions.Contains(OperationsPermissions.JobsManage) == true;
}

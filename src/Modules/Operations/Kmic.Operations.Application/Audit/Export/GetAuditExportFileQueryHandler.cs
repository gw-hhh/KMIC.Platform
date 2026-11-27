using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Operations.Contracts;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.Audit.Export;

public sealed class GetAuditExportFileQueryHandler(IModuleJobStores stores, IFileDownloads downloads, IExecutionContext execution, TimeProvider time)
    : IQueryHandler<GetAuditExportFileQuery, FileDownload>
{
    // 文件登记请求通常在数秒内完成；超时仍未查到时按文件不存在处理
    private static readonly TimeSpan _registrationWindow = TimeSpan.FromMinutes(5);

    public async ValueTask<Result<FileDownload>> Handle(GetAuditExportFileQuery query, CancellationToken cancellationToken)
    {
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

        // 作业完成时仅发出了异步登记命令，Files 模块落库前暂不可查
        var download = await downloads.GetAsync(fileId, cancellationToken);
        return download.IsFailure && download.Error.Type == ErrorType.NotFound && time.GetUtcNow() - job.FinishedAt < _registrationWindow
            ? OperationsErrors.AuditExportNotReady
            : download;
    }

    // 允许导出提交人或具备作业管理权限的用户访问
    private bool CanSee(JobView job) =>
        (execution.User.UserId is { } user && job.SubmittedBy == user.Value)
        || execution.Access?.Permissions.Contains(OperationsPermissions.JobsManage) == true;
}

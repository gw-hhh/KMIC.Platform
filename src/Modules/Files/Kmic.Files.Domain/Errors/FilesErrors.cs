using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Domain.Errors;

public static class FilesErrors
{
    public static readonly Error TooLarge = Error.PayloadTooLarge("Files.TooLarge", "文件大小超过上传限制");

    /// <summary>文件类型不允许、扩展名不支持或文件内容与扩展名不符</summary>
    public static readonly Error TypeNotAllowed = Error.UnsupportedMediaType("Files.TypeNotAllowed", "不支持该文件类型，或文件内容与扩展名不一致");

    public static readonly Error EmptyFile = Error.Validation("Files.EmptyFile", "文件为空");

    public static readonly Error ProfileNotFound = Error.Validation("Files.ProfileNotFound", "上传配置不存在");

    public static readonly Error NotFound = Error.NotFound("Files.NotFound", "文件不存在");

    public static readonly Error StorageIssueNotFound = Error.NotFound("Files.StorageIssueNotFound", "存储问题不存在");

    public static readonly Error NotReady = Error.Conflict("Files.NotReady", "文件尚未就绪，请稍后再试").WithMetadata(ErrorMetadataKeys.Retryable, true);

    public static readonly Error Purged = Error.Gone("Files.Purged", "文件已被删除");

    public static readonly Error TooManyFiles = Error.Validation("Files.TooManyFiles", "单次关联文件数超过上限");

    public static readonly Error NotAttachable = Error.BusinessRule("Files.NotAttachable", "文件不可关联，请检查上传人及文件状态");

    public static readonly Error TooManyReferences = Error.BusinessRule("Files.TooManyReferences", "文件引用数已达到上限");

    public static readonly Error InvalidState = Error.BusinessRule("Files.InvalidState", "文件当前状态不允许此操作");

    public static readonly Error UploadDenied = Error.Forbidden("Files.UploadDenied", "没有上传这类附件的权限");

    /// <summary>对账差异超过上限或未列出预期对象时返回，用于中止后续清理</summary>
    public static readonly Error ReconcileSuspicious = Error.BusinessRule("Files.ReconcileSuspicious", "对账结果异常，本次未处理文件。请检查存储配置及数据库是否曾被还原");
}

using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Domain.Errors;

/// <summary>
/// Files 模块的错误目录
/// </summary>
/// <remarks>
/// 错误码写成“Files.错误名”
/// </remarks>
public static class FilesErrors
{
    /// <summary>
    /// 文件超过了上传档案的大小上限
    /// </summary>
    public static readonly Error TooLarge = Error.PayloadTooLarge("Files.TooLarge", "文件太大，超过了这类附件允许的大小");

    /// <summary>
    /// 文件类型不被允许：扩展名不认识、内容与扩展名不符，或者这类附件不收这种类型
    /// </summary>
    public static readonly Error TypeNotAllowed = Error.UnsupportedMediaType("Files.TypeNotAllowed", "这类附件不允许上传这种文件，或者文件内容与扩展名不符");

    /// <summary>
    /// 文件是空的
    /// </summary>
    public static readonly Error EmptyFile = Error.Validation("Files.EmptyFile", "文件是空的");

    /// <summary>
    /// 上传档案不存在
    /// </summary>
    public static readonly Error ProfileNotFound = Error.Validation("Files.ProfileNotFound", "没有这类附件");

    /// <summary>
    /// 文件不存在
    /// </summary>
    public static readonly Error NotFound = Error.NotFound("Files.NotFound", "文件不存在");

    /// <summary>
    /// 存储问题不存在
    /// </summary>
    public static readonly Error StorageIssueNotFound = Error.NotFound("Files.StorageIssueNotFound", "存储问题不存在");

    /// <summary>
    /// 文件还没准备好：生成的文件还没登记，或者正在恢复
    /// </summary>
    public static readonly Error NotReady = Error.Conflict("Files.NotReady", "文件还没准备好，请稍后再试").WithMetadata(ErrorMetadataKeys.Retryable, true);

    /// <summary>
    /// 文件已经彻底删除
    /// </summary>
    public static readonly Error Purged = Error.Gone("Files.Purged", "文件已被删除");

    /// <summary>
    /// 一次关联的文件太多
    /// </summary>
    public static readonly Error TooManyFiles = Error.Validation("Files.TooManyFiles", "一次关联的文件太多");

    /// <summary>
    /// 文件不能关联：不是本人上传的、已经过期被清理、已经被拒绝
    /// </summary>
    public static readonly Error NotAttachable = Error.BusinessRule("Files.NotAttachable", "文件不能关联：不是本人上传的、已经过期，或者没有通过检查");

    /// <summary>
    /// 一个文件的引用太多
    /// </summary>
    public static readonly Error TooManyReferences = Error.BusinessRule("Files.TooManyReferences", "这个文件被引用的次数已经到达上限");

    /// <summary>
    /// 当前状态不能做这个操作
    /// </summary>
    public static readonly Error InvalidState = Error.BusinessRule("Files.InvalidState", "文件当前的状态不能做这个操作");

    /// <summary>
    /// 上传要有的权限点本人没有
    /// </summary>
    public static readonly Error UploadDenied = Error.Forbidden("Files.UploadDenied", "没有上传这类附件的权限");

    /// <summary>
    /// 对账发现的差异太多，没有做任何处理
    /// </summary>
    public static readonly Error ReconcileSuspicious = Error.BusinessRule("Files.ReconcileSuspicious", "对账发现的差异太多，没有做任何处理。请先检查存储的配置，以及数据库是否被还原过");
}

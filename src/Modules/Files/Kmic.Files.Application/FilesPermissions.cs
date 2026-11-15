using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Files.Application;

/// <summary>
/// Files 模块的权限点
/// </summary>
/// <remarks>
/// 上传与下载没有 Files 自己的权限点：上传由上传配置要求的权限决定，下载由业务对象的授权决定。
/// 使用敏感权限点的命令必须记录审计
/// </remarks>
[PermissionGroup("Files", "文件")]
public static class FilesPermissions
{
    /// <summary>
    /// 查看文件的元数据、引用和状态，不含内容
    /// </summary>
    [Permission("查看文件")]
    public const string FileView = "Files.File.View";

    /// <summary>
    /// 管理文件
    /// </summary>
    /// <remarks>
    /// 允许重试文件删除、将存储问题标为已解决
    /// </remarks>
    [Permission("管理文件")]
    public const string FileManage = "Files.File.Manage";

    /// <summary>
    /// 彻底删除文件：内容无法再读取，不可恢复
    /// </summary>
    [Permission("彻底删除文件", Sensitive = true)]
    public const string FilePurge = "Files.File.Purge";
}

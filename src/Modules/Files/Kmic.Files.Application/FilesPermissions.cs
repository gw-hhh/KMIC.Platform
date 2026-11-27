using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Files.Application;

/// <summary>Files 模块权限点（上传与下载不使用这些权限点）</summary>
[PermissionGroup("Files", "文件")]
public static class FilesPermissions
{
    /// <summary>查看文件的元数据、引用和状态，不含内容</summary>
    [Permission("查看文件")]
    public const string FileView = "Files.File.View";

    /// <summary>管理文件：重试文件删除、将存储问题标记为已解决</summary>
    [Permission("管理文件")]
    public const string FileManage = "Files.File.Manage";

    /// <summary>彻底删除文件：内容无法再读取，不可恢复</summary>
    [Permission("彻底删除文件", Sensitive = true)]
    public const string FilePurge = "Files.File.Purge";
}

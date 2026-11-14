using System.ComponentModel.DataAnnotations;
using Kmic.BuildingBlocks.Infrastructure.Files;

namespace Kmic.Files.Infrastructure.Storage;

/// <summary>
/// 文件存储部署配置，支持本地目录和 S3 兼容存储
/// </summary>
public sealed class FilesStorageOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Files:Storage";

    /// <summary>
    /// 存储键中的环境名，不得含“/”或“.”；用于隔离同一桶中的不同环境
    /// </summary>
    [Required]
    public string Environment { get; init; } = "dev";

    /// <summary>
    /// 存储提供程序：Local 为本地目录（开发、测试），S3 为兼容对象存储（生产使用阿里云 OSS）
    /// </summary>
    [Required]
    public string Provider { get; init; } = "Local";

    /// <summary>
    /// 本地存储的根目录，不能在站点目录里；空表示用系统临时目录下的 kmic-files
    /// </summary>
    public string LocalRoot { get; init; } = string.Empty;

    /// <summary>
    /// S3 连接配置，仅 Provider 为 S3 时使用；密钥从密钥源读取，不写入仓库
    /// </summary>
    public S3FileStorageOptions S3 { get; init; } = new();
}

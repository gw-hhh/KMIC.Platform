using System.ComponentModel.DataAnnotations;
using Kmic.BuildingBlocks.Infrastructure.Files;

namespace Kmic.Files.Infrastructure.Storage;

/// <summary>
/// 文件存储的部署配置：用本地目录还是 S3 兼容的对象存储
/// </summary>
public sealed class FilesStorageOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Files:Storage";

    /// <summary>
    /// 存储键里的环境名，不能含“/”或“.”：同一个桶可以放几个环境的文件而互不干扰
    /// </summary>
    [Required]
    public string Environment { get; init; } = "dev";

    /// <summary>
    /// 用哪种存储：Local 是本地目录（开发、测试），S3 是 S3 兼容的对象存储（生产是阿里云 OSS）
    /// </summary>
    [Required]
    public string Provider { get; init; } = "Local";

    /// <summary>
    /// 本地存储的根目录，不能在站点目录里；空表示用系统临时目录下的 kmic-files
    /// </summary>
    public string LocalRoot { get; init; } = string.Empty;

    /// <summary>
    /// S3 存储的连接设置，Provider 是 S3 时才用；密钥来自密钥源，不进仓库
    /// </summary>
    public S3FileStorageOptions S3 { get; init; } = new();
}

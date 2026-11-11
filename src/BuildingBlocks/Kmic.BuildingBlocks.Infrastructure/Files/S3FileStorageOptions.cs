namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// S3 兼容存储的连接配置
/// </summary>
public sealed class S3FileStorageOptions
{
    /// <summary>
    /// 服务地址：生产是阿里云 OSS 的内网 S3 兼容地址，开发和测试是 MinIO
    /// </summary>
    public string ServiceUrl { get; set; } = "";

    /// <summary>
    /// 桶名，私有桶，不开放匿名访问
    /// </summary>
    public string Bucket { get; set; } = "";

    /// <summary>
    /// 地域
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// 访问密钥 ID，来自密钥源，不能写在配置文件里
    /// </summary>
    public string AccessKeyId { get; set; } = "";

    /// <summary>
    /// 访问密钥，来自密钥源，不能写在配置文件里
    /// </summary>
    public string SecretAccessKey { get; set; } = "";

    /// <summary>
    /// 用“地址/桶名”的路径写法，MinIO 需要
    /// </summary>
    public bool ForcePathStyle { get; set; }

    /// <summary>
    /// 分段上传每段的大小（字节），小于这个大小的文件一次写完
    /// </summary>
    public int PartSizeBytes { get; set; } = 8 * 1024 * 1024;
}

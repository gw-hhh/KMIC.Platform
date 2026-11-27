namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>S3 兼容存储连接配置</summary>
public sealed class S3FileStorageOptions
{
    /// <summary>服务端点（生产环境为阿里云 OSS 内网 S3 地址，开发与测试为 MinIO）</summary>
    public string ServiceUrl { get; set; } = "";

    /// <summary>存储桶名称（私有桶，禁止匿名访问）</summary>
    public string Bucket { get; set; } = "";

    /// <summary>地域</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>访问密钥 ID（由密钥管理注入，禁止硬编码在配置里）</summary>
    public string AccessKeyId { get; set; } = "";

    /// <summary>访问密钥 Secret（由密钥管理注入，禁止硬编码在配置里）</summary>
    public string SecretAccessKey { get; set; } = "";

    /// <summary>是否使用路径风格访问（地址/桶名），MinIO 须开启</summary>
    public bool ForcePathStyle { get; set; }

    /// <summary>分段上传单片大小（字节），低于此值采用单次上传</summary>
    public int PartSizeBytes { get; set; } = 8 * 1024 * 1024;
}

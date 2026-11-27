namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>游标分页配置。</summary>
public sealed class CursorOptions
{
    public const string Section = "Kmic:Paging";

    /// <summary>游标签名密钥（Base64 编码，至少 32 字节），禁止写入配置文件；多实例部署须配置相同密钥。</summary>
    public string? SigningKey { get; init; }
}

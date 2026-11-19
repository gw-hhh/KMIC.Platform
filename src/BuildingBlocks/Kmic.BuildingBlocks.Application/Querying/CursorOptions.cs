namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 游标分页的配置
/// </summary>
public sealed class CursorOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Kmic:Paging";

    /// <summary>
    /// 游标签名密钥，Base64，至少 32 字节，不能写进配置文件
    /// </summary>
    /// <remarks>
    /// 多个实例要配同一个值
    /// </remarks>
    public string? SigningKey { get; init; }
}

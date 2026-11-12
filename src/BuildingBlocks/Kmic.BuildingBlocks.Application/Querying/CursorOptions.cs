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
    /// 游标签名用的密钥，Base64，至少 32 字节；这是密钥，只能来自用户机密或部署的密钥源，不能写进配置文件
    /// </summary>
    /// <remarks>
    /// 多个实例必须配置同一个值，否则一个实例签的游标换到另一个实例就无效。不配置时每个进程随机生成一个，
    /// 只适合开发和测试；生产环境不配置，宿主启动失败
    /// </remarks>
    public string? SigningKey { get; init; }
}

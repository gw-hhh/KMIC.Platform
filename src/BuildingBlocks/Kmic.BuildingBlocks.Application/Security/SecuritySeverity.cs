namespace Kmic.BuildingBlocks.Application.Security;

/// <summary>
/// 安全事件的严重程度
/// </summary>
public enum SecuritySeverity
{
    /// <summary>
    /// 一般记录
    /// </summary>
    Info,

    /// <summary>
    /// 值得关注
    /// </summary>
    Warning,

    /// <summary>
    /// 需要立即处理
    /// </summary>
    High,
}

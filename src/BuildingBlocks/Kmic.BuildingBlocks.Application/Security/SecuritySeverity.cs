namespace Kmic.BuildingBlocks.Application.Security;

/// <summary>
/// 严重程度
/// </summary>
public enum SecuritySeverity
{
    /// <summary>
    /// 一般
    /// </summary>
    Info,

    /// <summary>
    /// 需要关注
    /// </summary>
    Warning,

    /// <summary>
    /// 需要立即处理
    /// </summary>
    High,
}

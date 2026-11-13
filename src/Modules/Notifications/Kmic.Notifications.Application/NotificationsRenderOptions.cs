using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Application;

/// <summary>
/// 模板渲染的上限
/// </summary>
/// <remarks>
/// 部署配置，节名 Notifications:Template
/// </remarks>
public sealed class NotificationsRenderOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Notifications:Template";

    /// <summary>
    /// 单次渲染最多用多少毫秒
    /// </summary>
    [Range(50, 30_000)]
    public int RenderTimeoutMs { get; init; } = 1000;

    /// <summary>
    /// 单次渲染的输出最多多少 KB
    /// </summary>
    [Range(1, 10_240)]
    public int MaxOutputKb { get; init; } = 1024;

    /// <summary>
    /// 单次渲染最多执行多少步：循环再大也有上限
    /// </summary>
    [Range(100, 10_000_000)]
    public int MaxSteps { get; init; } = 100_000;
}

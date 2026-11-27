using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Application;

/// <summary>模板渲染资源限制配置</summary>
public sealed class NotificationsRenderOptions
{
    public const string Section = "Notifications:Template";

    /// <summary>单次渲染时限（毫秒）</summary>
    [Range(50, 30_000)]
    public int RenderTimeoutMs { get; init; } = 1000;

    /// <summary>单次渲染输出长度上限（1024 字符为单位）</summary>
    [Range(1, 10_240)]
    public int MaxOutputKb { get; init; } = 1024;

    /// <summary>单次渲染执行步数上限（限制模板循环）</summary>
    [Range(100, 10_000_000)]
    public int MaxSteps { get; init; } = 100_000;
}

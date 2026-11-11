using System.Diagnostics;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>
/// 框架的追踪来源
/// </summary>
public static class KmicActivities
{
    /// <summary>
    /// 追踪来源名称
    /// </summary>
    public const string MessagingName = "Kmic.Messaging";

    /// <summary>
    /// 追踪来源
    /// </summary>
    public static readonly ActivitySource Messaging = new(MessagingName);
}

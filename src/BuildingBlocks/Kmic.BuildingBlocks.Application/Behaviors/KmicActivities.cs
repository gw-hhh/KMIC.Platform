using System.Diagnostics;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>框架分布式追踪来源。</summary>
public static class KmicActivities
{
    public const string MessagingName = "Kmic.Messaging";

    public static readonly ActivitySource Messaging = new(MessagingName);
}

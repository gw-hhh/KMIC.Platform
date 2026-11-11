namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 值对象集合的元素，整个集合存为一个 jsonb 列
/// </summary>
internal sealed record Watcher
{
    private Watcher(string name, TicketPriority notifyFrom)
    {
        Name = name;
        NotifyFrom = notifyFrom;
    }

    public string Name { get; private init; }

    public TicketPriority NotifyFrom { get; private init; }

    public static Watcher Of(string name, TicketPriority notifyFrom) => new(name, notifyFrom);
}

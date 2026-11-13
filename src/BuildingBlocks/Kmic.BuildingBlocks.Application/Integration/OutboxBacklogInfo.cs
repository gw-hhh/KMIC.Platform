namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 发件箱的积压
/// </summary>
/// <param name="Pending">还没投递完的条数：待投递加投递中</param>
/// <param name="OldestAge">最老的一条已经发布了多久；没有积压时为零</param>
/// <param name="Dead">死信条数：投递一直失败，次数用尽</param>
public sealed record OutboxBacklogInfo(long Pending, TimeSpan OldestAge, long Dead);

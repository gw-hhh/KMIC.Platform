namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 带聚合版本的命令收据：事务行为在保存之后用聚合的新版本回填，处理器按当前值填写即可
/// </summary>
public interface IHasConcurrencyVersion
{
    /// <summary>
    /// 版本所属的聚合
    /// </summary>
    Guid AggregateId { get; }

    /// <summary>
    /// 聚合的并发版本
    /// </summary>
    long ConcurrencyVersion { get; }

    /// <summary>
    /// 换上新版本的收据
    /// </summary>
    IHasConcurrencyVersion WithConcurrencyVersion(long version);
}

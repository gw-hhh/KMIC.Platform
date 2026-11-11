namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 带版本号的命令结果
/// </summary>
/// <remarks>
/// 保存后会换成新版本号，处理器按当前值填写即可
/// </remarks>
public interface IHasConcurrencyVersion
{
    /// <summary>
    /// 版本号所属的聚合根
    /// </summary>
    Guid AggregateId { get; }

    /// <summary>
    /// 版本号
    /// </summary>
    long ConcurrencyVersion { get; }

    /// <summary>
    /// 换成新版本号
    /// </summary>
    IHasConcurrencyVersion WithConcurrencyVersion(long version);
}

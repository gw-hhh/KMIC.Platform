namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>带并发版本号的命令结果；持久化后将自动替换为新版本号，处理器按当前值填充即可。</summary>
public interface IHasConcurrencyVersion
{
    Guid AggregateId { get; }

    long ConcurrencyVersion { get; }

    IHasConcurrencyVersion WithConcurrencyVersion(long version);
}

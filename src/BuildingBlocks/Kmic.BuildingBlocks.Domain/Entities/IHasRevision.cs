namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>标识数据包含业务版本号（如报价的 V1、V2）。</summary>
public interface IHasRevision
{
    /// <summary>业务版本号（与并发版本号无关）。</summary>
    int Revision { get; }
}

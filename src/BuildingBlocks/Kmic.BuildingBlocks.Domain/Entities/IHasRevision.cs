namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 有业务版本的数据，如报价的 V1、V2
/// </summary>
public interface IHasRevision
{
    /// <summary>
    /// 业务版本号，和并发版本号无关
    /// </summary>
    int Revision { get; }
}

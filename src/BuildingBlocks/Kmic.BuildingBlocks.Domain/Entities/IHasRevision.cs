namespace Kmic.BuildingBlocks.Domain.Entities;

/// <summary>
/// 有业务版本（如报价的 V1、V2）的聚合；业务历史会自动带上当时的版本
/// </summary>
public interface IHasRevision
{
    /// <summary>
    /// 业务版本号，只在业务上产生新版本时递增，与并发版本无关
    /// </summary>
    int Revision { get; }
}

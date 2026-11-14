using Kmic.BuildingBlocks.Application.Time;

namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 权限点目录的部署配置
/// </summary>
public sealed class CatalogOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Identity:Catalog";

    /// <summary>
    /// 退役权限点及范围对象的保留时长，用于在部署回滚时保留角色配置
    /// </summary>
    [TimeSpanRange("1.00:00:00", "3650.00:00:00")]
    public TimeSpan RetiredRetention { get; init; } = TimeSpan.FromDays(180);
}

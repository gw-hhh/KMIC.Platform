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
    /// 退役的权限点与范围对象保留多久：部署回滚要能找回角色上的配置
    /// </summary>
    [TimeSpanRange("1.00:00:00", "3650.00:00:00")]
    public TimeSpan RetiredRetention { get; init; } = TimeSpan.FromDays(180);
}

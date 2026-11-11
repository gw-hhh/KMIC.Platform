namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 框架在数据库中建立的排序规则
/// </summary>
public static class Collations
{
    /// <summary>
    /// 不区分大小写（ICU und-u-ks-level2，非确定性），建在 public，由迁移程序在模块迁移之前创建并核对
    /// </summary>
    public const string CaseInsensitive = "CaseInsensitive";
}

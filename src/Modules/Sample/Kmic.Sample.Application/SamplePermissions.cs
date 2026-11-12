using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Sample.Application;

/// <summary>
/// Sample 模块的权限点
/// </summary>
[PermissionGroup("Sample", "示例标本")]
public static class SamplePermissions
{
    /// <summary>
    /// 查看
    /// </summary>
    [Permission("查看示例标本")]
    public const string View = "Sample.Specimen.View";

    /// <summary>
    /// 创建
    /// </summary>
    [Permission("创建示例标本")]
    public const string Create = "Sample.Specimen.Create";

    /// <summary>
    /// 提交
    /// </summary>
    [Permission("提交示例标本")]
    public const string Submit = "Sample.Specimen.Submit";

    /// <summary>
    /// 关闭
    /// </summary>
    [Permission("关闭示例标本", RenamedFrom = "Sample.Specimen.Finish")]
    public const string Close = "Sample.Specimen.Close";

    /// <summary>
    /// 删除
    /// </summary>
    [Permission("删除示例标本", Sensitive = true)]
    public const string Delete = "Sample.Specimen.Delete";

    /// <summary>
    /// 修改示例设置
    /// </summary>
    [Permission("管理示例设置")]
    public const string SettingManage = "Sample.Setting.Manage";
}

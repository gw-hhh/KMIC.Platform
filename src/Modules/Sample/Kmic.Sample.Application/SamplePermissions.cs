using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Sample.Application;

[PermissionGroup("Sample", "示例")]
public static class SamplePermissions
{
    [Permission("查看示例")]
    public const string View = "Sample.Specimen.View";

    [Permission("创建示例")]
    public const string Create = "Sample.Specimen.Create";

    [Permission("编辑示例附件")]
    public const string Edit = "Sample.Specimen.Edit";

    [Permission("提交示例")]
    public const string Submit = "Sample.Specimen.Submit";

    [Permission("关闭示例", RenamedFrom = "Sample.Specimen.Finish")]
    public const string Close = "Sample.Specimen.Close";

    [Permission("删除示例", Sensitive = true)]
    public const string Delete = "Sample.Specimen.Delete";

    [Permission("管理示例设置")]
    public const string SettingManage = "Sample.Setting.Manage";

    [Permission("维护示例分类")]
    public const string CategoryManage = "Sample.Category.Manage";
}

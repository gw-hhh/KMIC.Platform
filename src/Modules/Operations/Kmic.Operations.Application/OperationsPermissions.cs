using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Operations.Application;

/// <summary>
/// Operations 模块的权限点
/// </summary>
/// <remarks>
/// 每个权限点是一个常量，写成“Operations.资源.动作”，并标 [Permission("显示名")]。
/// 权限点随用到它的用例一起加入：声明了却没有用例使用，宿主启动时会拒绝
/// </remarks>
[PermissionGroup("Operations", "运维")]
public static class OperationsPermissions
{
    /// <summary>
    /// 查看审计：能看到全部人的操作记录和字段变更，所以是敏感权限点，查询本身也记读取审计
    /// </summary>
    [Permission("查看审计", Sensitive = true)]
    public const string AuditView = "Operations.Audit.View";
}

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

    /// <summary>
    /// 查看定时任务与它们的执行记录、全部后台作业
    /// </summary>
    [Permission("查看定时任务与后台作业")]
    public const string JobsView = "Operations.Jobs.View";

    /// <summary>
    /// 改定时任务的计划、立即执行、取消后台作业；用到它的命令都记审计
    /// </summary>
    [Permission("管理定时任务与后台作业")]
    public const string JobsManage = "Operations.Jobs.Manage";

    /// <summary>
    /// 查看消息积压、死信、被挡住的顺序键；查看死信载荷还会留下读取审计
    /// </summary>
    [Permission("查看消息")]
    public const string MessagesView = "Operations.Messages.View";

    /// <summary>
    /// 重试、放弃死信：放弃意味着消息永远不会被处理，所以是敏感权限点，用到它的命令都记审计
    /// </summary>
    [Permission("管理消息", Sensitive = true)]
    public const string MessagesManage = "Operations.Messages.Manage";
}

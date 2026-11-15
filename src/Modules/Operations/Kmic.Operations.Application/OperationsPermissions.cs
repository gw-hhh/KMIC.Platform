using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Operations.Application;

/// <summary>
/// Operations 模块的权限点
/// </summary>
/// <remarks>
/// 未被用例或资源动作使用的权限点会导致启动失败
/// </remarks>
[PermissionGroup("Operations", "运维")]
public static class OperationsPermissions
{
    /// <summary>
    /// 查看审计记录
    /// </summary>
    /// <remarks>
    /// 可查看所有人的操作及字段变更，查询须记录读审计
    /// </remarks>
    [Permission("查看审计", Sensitive = true)]
    public const string AuditView = "Operations.Audit.View";

    /// <summary>
    /// 导出审计记录
    /// </summary>
    /// <remarks>
    /// 可导出所有人的操作记录，相关命令须记录审计
    /// </remarks>
    [Permission("导出审计", Sensitive = true)]
    public const string AuditExport = "Operations.Audit.Export";

    /// <summary>
    /// 查看定时任务与它们的执行记录、全部后台作业
    /// </summary>
    [Permission("查看定时任务与后台作业")]
    public const string JobsView = "Operations.Jobs.View";

    /// <summary>
    /// 修改定时计划、手动触发任务、取消后台作业；使用该权限的命令须审计
    /// </summary>
    [Permission("管理定时任务与后台作业")]
    public const string JobsManage = "Operations.Jobs.Manage";

    /// <summary>
    /// 查看消息运行情况
    /// </summary>
    /// <remarks>
    /// 包括积压、死信、阻塞键、路由、实例和读模型重建记录；查看死信载荷会记录读审计
    /// </remarks>
    [Permission("查看消息")]
    public const string MessagesView = "Operations.Messages.View";

    /// <summary>
    /// 管理消息
    /// </summary>
    /// <remarks>
    /// 包括处理死信、管理路由、重新投递和重建读模型；放弃死信后不再处理，相关命令须记录审计
    /// </remarks>
    [Permission("管理消息", Sensitive = true)]
    public const string MessagesManage = "Operations.Messages.Manage";
}

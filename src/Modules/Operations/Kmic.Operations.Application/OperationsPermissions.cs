using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Operations.Application;

/// <summary>
/// Operations 模块的权限点
/// </summary>
[PermissionGroup("Operations", "运维")]
public static class OperationsPermissions
{
    /// <summary>
    /// 查看审计记录，包括所有人的操作和字段变更
    /// </summary>
    [Permission("查看审计", Sensitive = true)]
    public const string AuditView = "Operations.Audit.View";

    /// <summary>
    /// 导出审计记录，包括所有人的操作
    /// </summary>
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
    /// 查看消息运行情况：积压、死信、阻塞键、路由、实例和读模型重建记录
    /// </summary>
    [Permission("查看消息")]
    public const string MessagesView = "Operations.Messages.View";

    /// <summary>
    /// 管理消息：处理死信、管理路由、重新投递和重建读模型
    /// </summary>
    [Permission("管理消息", Sensitive = true)]
    public const string MessagesManage = "Operations.Messages.Manage";

    /// <summary>
    /// 修改数据库已分配容量的设置：设错会让消息写入被误拒绝，或失去保护
    /// </summary>
    [Permission("管理消息容量设置", Sensitive = true)]
    public const string MessagingCapacityManage = "Operations.MessagingCapacity.Manage";
}

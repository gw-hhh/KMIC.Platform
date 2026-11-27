using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Operations.Application;

[PermissionGroup("Operations", "运维")]
public static class OperationsPermissions
{
    /// <summary>查看审计记录（含所有用户的操作与字段变更）。</summary>
    [Permission("查看审计", Sensitive = true)]
    public const string AuditView = "Operations.Audit.View";

    /// <summary>导出审计记录（含所有用户的操作）。</summary>
    [Permission("导出审计", Sensitive = true)]
    public const string AuditExport = "Operations.Audit.Export";

    /// <summary>查看定时任务、执行记录与全部后台作业。</summary>
    [Permission("查看定时任务与后台作业")]
    public const string JobsView = "Operations.Jobs.View";

    /// <summary>修改定时计划、手动触发任务、取消后台作业（使用该权限的命令须审计）。</summary>
    [Permission("管理定时任务与后台作业")]
    public const string JobsManage = "Operations.Jobs.Manage";

    /// <summary>查看消息积压、死信、阻塞键、路由、实例与读模型重建记录。</summary>
    [Permission("查看消息")]
    public const string MessagesView = "Operations.Messages.View";

    /// <summary>处理死信、管理路由、重新投递与重建读模型。</summary>
    [Permission("管理消息", Sensitive = true)]
    public const string MessagesManage = "Operations.Messages.Manage";

    /// <summary>修改数据库已分配容量配置（配置不当可能导致消息写入被误拒或容量保护失效）。</summary>
    [Permission("管理消息容量设置", Sensitive = true)]
    public const string MessagingCapacityManage = "Operations.MessagingCapacity.Manage";
}

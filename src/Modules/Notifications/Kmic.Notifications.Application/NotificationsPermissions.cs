using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Notifications.Application;

/// <summary>Notifications 模块权限点</summary>
[PermissionGroup("Notifications", "通知")]
public static class NotificationsPermissions
{
    /// <summary>查看模板、版本历史及预览渲染结果</summary>
    [Permission("查看通知模板")]
    public const string TemplateView = "Notifications.Template.View";

    /// <summary>发布模板新版本及恢复默认内容</summary>
    [Permission("管理通知模板")]
    public const string TemplateManage = "Notifications.Template.Manage";

    /// <summary>查看投递记录与正文（须记录读取审计）</summary>
    [Permission("查看通知投递记录", Sensitive = true)]
    public const string DeliveryView = "Notifications.Delivery.View";

    /// <summary>重试或放弃失败投递</summary>
    [Permission("处理失败的通知投递", Sensitive = true)]
    public const string DeliveryManage = "Notifications.Delivery.Manage";

    /// <summary>查看捕获内容（仅捕获模式下可用）</summary>
    [Permission("查看捕获的通知内容", Sensitive = true)]
    public const string CapturedView = "Notifications.Captured.View";
}

using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Notifications.Application;

/// <summary>
/// Notifications 模块的权限点
/// </summary>
/// <remarks>
/// 站内消息按收件人控制访问，不另设权限点
/// </remarks>
[PermissionGroup("Notifications", "通知")]
public static class NotificationsPermissions
{
    /// <summary>
    /// 查看通知模板、版本历史，预览渲染结果
    /// </summary>
    [Permission("查看通知模板")]
    public const string TemplateView = "Notifications.Template.View";

    /// <summary>
    /// 发布模板的新版本，恢复默认内容
    /// </summary>
    [Permission("管理通知模板")]
    public const string TemplateManage = "Notifications.Template.Manage";

    /// <summary>
    /// 查看投递记录及正文，须记录读审计
    /// </summary>
    [Permission("查看通知投递记录", Sensitive = true)]
    public const string DeliveryView = "Notifications.Delivery.View";

    /// <summary>
    /// 重试、放弃失败的投递
    /// </summary>
    [Permission("处理失败的通知投递", Sensitive = true)]
    public const string DeliveryManage = "Notifications.Delivery.Manage";

    /// <summary>
    /// 查看捕获的通知内容
    /// </summary>
    /// <remarks>
    /// 仅开启捕获时可用，内容可能含验证码
    /// </remarks>
    [Permission("查看捕获的通知内容", Sensitive = true)]
    public const string CapturedView = "Notifications.Captured.View";
}

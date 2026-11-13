using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.Notifications.Application;

/// <summary>
/// Notifications 模块的权限点
/// </summary>
/// <remarks>
/// 每个权限点是一个常量，写成“Notifications.资源.动作”，并标 [Permission("显示名")]。站内消息只按收件人访问，没有权限点
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
    /// 查看投递记录和正文；记录里有收件人和通知内容，查看会写读审计
    /// </summary>
    [Permission("查看通知投递记录", Sensitive = true)]
    public const string DeliveryView = "Notifications.Delivery.View";

    /// <summary>
    /// 重试、放弃失败的投递
    /// </summary>
    [Permission("处理失败的通知投递", Sensitive = true)]
    public const string DeliveryManage = "Notifications.Delivery.Manage";

    /// <summary>
    /// 查看非生产环境捕获的通知内容；可能含验证码，生产环境没有这些数据
    /// </summary>
    [Permission("查看捕获的通知内容", Sensitive = true)]
    public const string CapturedView = "Notifications.Captured.View";
}

namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>
/// 通知模板的数据模型
/// </summary>
/// <remarks>
/// 用不可变 record 实现，同时标 <see cref="NotificationTemplateAttribute" />。
/// 模型里的属性就是模板里能用的变量，值都要在发送时准备好，渲染时不再查任何数据
/// </remarks>
/// <typeparam name="TSelf">模型自身</typeparam>
public interface INotificationModel<TSelf>
    where TSelf : INotificationModel<TSelf>
{
    /// <summary>
    /// 示例数据：管理页预览和启动时检查默认内容能否渲染都用它
    /// </summary>
    static abstract TSelf Example { get; }

    /// <summary>
    /// 默认主题；站内消息里是标题
    /// </summary>
    static abstract string DefaultSubject { get; }

    /// <summary>
    /// 默认正文；邮件用，可以是 HTML
    /// </summary>
    static abstract string DefaultBody { get; }

    /// <summary>
    /// 短信与站内消息的默认正文，必须是纯文本；不写时沿用 <see cref="DefaultBody" />
    /// </summary>
    static virtual string DefaultTextBody => TSelf.DefaultBody;
}

namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>通知模板的数据模型；所有字段值须在发送前准备完毕，模板渲染阶段不再回查数据。</summary>
/// <typeparam name="TSelf">模板模型自身类型</typeparam>
public interface INotificationModel<TSelf>
    where TSelf : INotificationModel<TSelf>
{
    /// <summary>示例数据，供管理页预览及启动时校验默认模板渲染。</summary>
    static abstract TSelf Example { get; }

    /// <summary>默认主题（站内消息中作为标题）。</summary>
    static abstract string DefaultSubject { get; }

    /// <summary>默认正文（邮件渠道支持 HTML）。</summary>
    static abstract string DefaultBody { get; }

    /// <summary>纯文本默认正文，供短信与站内消息使用；未指定时沿用 <see cref="DefaultBody" />。</summary>
    static virtual string DefaultTextBody => TSelf.DefaultBody;
}

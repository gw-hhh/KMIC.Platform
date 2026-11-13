namespace Kmic.Notifications.Application.Templates.Get;

/// <summary>
/// 发布过的一个版本
/// </summary>
/// <param name="Version">版本号</param>
/// <param name="Channel">渠道编码</param>
/// <param name="Subject">主题</param>
/// <param name="Body">正文</param>
/// <param name="PublishedBy">发布人</param>
/// <param name="PublishedAt">发布时间</param>
/// <param name="Remark">备注</param>
public sealed record TemplateVersionInfo(int Version, string Channel, string Subject, string Body, Guid PublishedBy, DateTimeOffset PublishedAt, string? Remark);

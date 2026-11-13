using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages;

/// <summary>
/// 死信列表里的一条
/// </summary>
/// <param name="Id">行 ID；重试、放弃、查看内容都用它。收件箱里是收件箱行的 ID，不是消息 ID</param>
/// <param name="Box">在发件箱还是收件箱</param>
/// <param name="Module">所在模块</param>
/// <param name="Subscription">订阅名；发件箱里没有</param>
/// <param name="MessageId">消息 ID</param>
/// <param name="MessageType">消息名称</param>
/// <param name="SchemaVersion">消息版本</param>
/// <param name="OccurredAt">消息发生的时间</param>
/// <param name="Attempts">已经试了几次</param>
/// <param name="LastError">最近一次的错误</param>
public sealed record DeadMessageItem(
    Guid Id,
    MessageBoxKind Box,
    string Module,
    string? Subscription,
    Guid MessageId,
    string MessageType,
    int SchemaVersion,
    DateTimeOffset OccurredAt,
    int Attempts,
    string? LastError);

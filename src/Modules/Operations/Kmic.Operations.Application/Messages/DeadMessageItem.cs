using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages;

/// <summary>死信列表项。</summary>
/// <param name="Id">行 ID</param>
/// <param name="Box">所属消息箱（发件箱或收件箱）</param>
/// <param name="Module">所在模块</param>
/// <param name="Subscription">订阅名</param>
/// <param name="MessageId">消息 ID</param>
/// <param name="MessageType">消息名称</param>
/// <param name="SchemaVersion">消息版本</param>
/// <param name="OccurredAt">消息发生时间</param>
/// <param name="Attempts">处理尝试次数</param>
/// <param name="LastError">最近一次错误信息</param>
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

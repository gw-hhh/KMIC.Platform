using System.ComponentModel;

namespace Kmic.Operations.Presentation.Messages;

/// <param name="Module">模块筛选</param>
/// <param name="Box">消息箱：Outbox 或 Inbox</param>
/// <param name="Subscription">订阅名</param>
/// <param name="MessageType">消息名称</param>
/// <param name="After">上一页返回的游标，首页不传</param>
/// <param name="Limit">每页数量</param>
public sealed record ListDeadMessagesRequest(
    string? Module,
    string? Box,
    string? Subscription,
    string? MessageType,
    string? After,
    [property: DefaultValue(50)] int Limit = 50);

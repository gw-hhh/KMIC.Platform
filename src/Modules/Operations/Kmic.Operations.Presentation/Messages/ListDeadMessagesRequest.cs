using System.ComponentModel;

namespace Kmic.Operations.Presentation.Messages;

/// <summary>
/// 查询死信的参数
/// </summary>
/// <param name="Module">只看这个模块的；不给就是全部模块</param>
/// <param name="Box">Outbox 或 Inbox</param>
/// <param name="Subscription">订阅名；给了就只看收件箱</param>
/// <param name="MessageType">消息名称</param>
/// <param name="After">上一页返回的游标；第一页不给</param>
/// <param name="Limit">每页数量，1 到 200，默认 50</param>
public sealed record ListDeadMessagesRequest(
    string? Module,
    string? Box,
    string? Subscription,
    string? MessageType,
    string? After,
    [property: DefaultValue(50)] int Limit = 50);

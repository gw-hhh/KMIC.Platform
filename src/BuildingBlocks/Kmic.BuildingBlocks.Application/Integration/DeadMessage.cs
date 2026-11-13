namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 一条死信：处理或投递一直失败，等运维重试或放弃
/// </summary>
/// <param name="Id">行 ID；收件箱里是收件箱行的 ID，不是消息 ID</param>
/// <param name="Box">在发件箱还是收件箱</param>
/// <param name="Module">所在模块</param>
/// <param name="Subscription">订阅名；发件箱里没有</param>
/// <param name="MessageId">消息 ID</param>
/// <param name="MessageType">消息名称</param>
/// <param name="SchemaVersion">消息版本</param>
/// <param name="OccurredAt">消息发生的时间</param>
/// <param name="Attempts">已经试了几次</param>
/// <param name="LastError">最近一次的错误</param>
public sealed record DeadMessage(
    Guid Id,
    MessageBox Box,
    string Module,
    string? Subscription,
    Guid MessageId,
    string MessageType,
    int SchemaVersion,
    DateTimeOffset OccurredAt,
    int Attempts,
    string? LastError);

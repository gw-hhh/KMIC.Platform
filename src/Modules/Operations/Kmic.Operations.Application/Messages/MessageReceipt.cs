using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages;

/// <summary>
/// 消息重试或放弃结果，各条结果独立，单条失败不影响其他消息
/// </summary>
/// <param name="Module">所在模块</param>
/// <param name="Box">在发件箱还是收件箱</param>
/// <param name="Id">行 ID</param>
/// <param name="Status">结果</param>
/// <param name="Reason">未成功原因，放弃检查拒绝时返回接收方提供的说明</param>
public sealed record MessageReceipt(string Module, MessageBoxKind Box, Guid Id, MessageReceiptStatus Status, string? Reason);

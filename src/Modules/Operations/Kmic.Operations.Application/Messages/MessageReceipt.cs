using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages;

/// <summary>
/// 重试或放弃一条消息的回执：每条各有各的结果，一条失败不影响别的
/// </summary>
/// <param name="Module">所在模块</param>
/// <param name="Box">在发件箱还是收件箱</param>
/// <param name="Id">行 ID</param>
/// <param name="Status">结果</param>
/// <param name="Reason">没做成时的原因：接收方的放弃检查不让放弃时是它给的说明</param>
public sealed record MessageReceipt(string Module, MessageBoxKind Box, Guid Id, MessageReceiptStatus Status, string? Reason);

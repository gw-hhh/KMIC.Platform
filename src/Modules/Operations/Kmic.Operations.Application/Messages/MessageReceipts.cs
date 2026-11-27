namespace Kmic.Operations.Application.Messages;

/// <summary>重试或放弃一批消息的回执。</summary>
/// <param name="Receipts">逐条对应的处理回执</param>
public sealed record MessageReceipts(IReadOnlyList<MessageReceipt> Receipts);

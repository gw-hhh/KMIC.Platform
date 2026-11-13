namespace Kmic.Operations.Application.Messages;

/// <summary>
/// 重试或放弃一批消息的回执
/// </summary>
/// <param name="Receipts">每条一个回执，顺序与请求一致</param>
public sealed record MessageReceipts(IReadOnlyList<MessageReceipt> Receipts);

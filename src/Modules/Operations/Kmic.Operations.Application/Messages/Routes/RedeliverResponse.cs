namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 补投的结果
/// </summary>
/// <param name="Candidates">区间内符合补投条件的消息数（已投递）</param>
/// <param name="Written">实际写入收件箱的消息数，不含重复消息</param>
public sealed record RedeliverResponse(long Candidates, long Written);

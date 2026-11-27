namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>补投结果。</summary>
/// <param name="Candidates">符合补投条件的消息数（区间内已投递）</param>
/// <param name="Written">实际写入收件箱的消息数</param>
public sealed record RedeliverResponse(long Candidates, long Written);

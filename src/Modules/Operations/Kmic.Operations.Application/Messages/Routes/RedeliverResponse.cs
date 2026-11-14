namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 补投的结果
/// </summary>
/// <param name="Candidates">区间里符合条件（已经投递过）的消息条数</param>
/// <param name="Written">真正写进订阅收件箱的条数，订阅已经收到过的不再写</param>
public sealed record RedeliverResponse(long Candidates, long Written);

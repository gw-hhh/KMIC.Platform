namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>终结订阅的结果。</summary>
/// <param name="Abandoned">终结时放弃的未完成消息数</param>
public sealed record FinalizeRouteResponse(long Abandoned);

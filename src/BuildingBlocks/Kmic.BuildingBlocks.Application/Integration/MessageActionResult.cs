namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 对一条消息做重试或放弃的结果
/// </summary>
/// <param name="Id">行 ID</param>
/// <param name="Outcome">结果</param>
public sealed record MessageActionResult(Guid Id, MessageActionOutcome Outcome);

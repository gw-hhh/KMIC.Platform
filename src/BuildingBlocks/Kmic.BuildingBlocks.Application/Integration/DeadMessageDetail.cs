namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>死信详情，含载荷与信封附加头字段。</summary>
public sealed record DeadMessageDetail(DeadMessage Message, DeadMessageState State, string Payload, string Headers);

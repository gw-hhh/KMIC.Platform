namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 信封里不单独成列的字段
/// </summary>
/// <param name="SourceModule">发布消息的模块</param>
/// <param name="InitiatorUserId">发起这件事的用户，系统身份处理事件时沿用原来的用户</param>
/// <param name="ActualUserId">模拟登录时的实际操作人</param>
/// <param name="CausationId">引发它的消息 ID，在处理别的消息时发布才有</param>
/// <param name="TraceParent">发布时的追踪上下文</param>
/// <param name="TraceState">发布时的追踪状态</param>
/// <param name="Purged">快照事件：源数据已物理删除</param>
internal sealed record MessageHeaders(
    string SourceModule,
    Guid? InitiatorUserId,
    Guid? ActualUserId,
    Guid? CausationId,
    string? TraceParent,
    string? TraceState,
    bool Purged);

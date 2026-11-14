using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Sample.Contracts.Events;

/// <summary>
/// 示例已提交，其他模块据此跟进后续流程
/// </summary>
/// <remarks>
/// 和领域事件同名，用命名空间区分。载荷只用 Guid 和基础类型，契约里没有领域类型
/// </remarks>
/// <param name="SpecimenId">示例 ID</param>
/// <param name="Number">示例编号</param>
[IntegrationMessage("Sample.SpecimenSubmitted")]
public sealed record SpecimenSubmitted(Guid SpecimenId, string Number) : IIntegrationEvent;

using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Sample.Contracts.Events;

/// <summary>示例已提交事件，供其他模块跟进后续流程</summary>
[IntegrationMessage("Sample.SpecimenSubmitted")]
public sealed record SpecimenSubmitted(Guid SpecimenId, string Number) : IIntegrationEvent;

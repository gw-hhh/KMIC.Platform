using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Sample.Domain.Specimens.Events;

/// <summary>示例已提交领域事件</summary>
public sealed record SpecimenSubmitted(SpecimenId Id, string Number, string Title, UserId OwnerId) : IDomainEvent;

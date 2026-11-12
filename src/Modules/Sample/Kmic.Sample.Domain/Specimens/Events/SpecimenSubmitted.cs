using Kmic.BuildingBlocks.Domain.Events;

namespace Kmic.Sample.Domain.Specimens.Events;

/// <summary>
/// 示例标本已提交
/// </summary>
/// <param name="Id">标本</param>
public sealed record SpecimenSubmitted(SpecimenId Id) : IDomainEvent;

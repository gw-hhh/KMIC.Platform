using Kmic.BuildingBlocks.Domain.Events;

namespace Kmic.Sample.Domain.Specimens.Events;

/// <summary>
/// 示例标本已提交
/// </summary>
/// <param name="Id">标本</param>
/// <param name="Number">标本编号，转成集成事件时带给订阅方</param>
public sealed record SpecimenSubmitted(SpecimenId Id, string Number) : IDomainEvent;

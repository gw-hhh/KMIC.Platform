using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Sample.Domain.Specimens.Events;

/// <summary>
/// 示例标本已提交
/// </summary>
/// <param name="Id">标本</param>
/// <param name="Number">标本编号，转成集成事件时带给订阅方</param>
/// <param name="Title">标本标题，通知负责人时用</param>
/// <param name="OwnerId">负责人，提交之后通知他</param>
public sealed record SpecimenSubmitted(SpecimenId Id, string Number, string Title, UserId OwnerId) : IDomainEvent;

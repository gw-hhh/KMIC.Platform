using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Sample.Domain.Specimens.Events;

/// <summary>
/// 示例已提交
/// </summary>
/// <param name="Id">示例 ID</param>
/// <param name="Number">示例编号，随集成事件传给订阅方</param>
/// <param name="Title">示例标题，用于通知负责人</param>
/// <param name="OwnerId">负责人 ID，提交后接收通知</param>
public sealed record SpecimenSubmitted(SpecimenId Id, string Number, string Title, UserId OwnerId) : IDomainEvent;

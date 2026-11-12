using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Domain.Users.Events;

/// <summary>
/// 用户已被禁用，同一事务里作废他的全部会话
/// </summary>
/// <param name="UserId">用户</param>
public sealed record UserDisabled(UserId UserId) : IDomainEvent;

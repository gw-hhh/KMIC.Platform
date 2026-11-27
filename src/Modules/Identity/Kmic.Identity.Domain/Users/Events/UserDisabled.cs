using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Domain.Users.Events;

/// <summary>用户已禁用，在同一事务内作废该用户的全部会话。</summary>
public sealed record UserDisabled(UserId UserId) : IDomainEvent;

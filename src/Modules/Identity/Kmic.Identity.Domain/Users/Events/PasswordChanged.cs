using Kmic.BuildingBlocks.Domain.Events;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Domain.Users.Events;

/// <summary>用户密码已变更（自主修改或管理员重置）。</summary>
/// <param name="ByAdministrator">是否由管理员重置</param>
public sealed record PasswordChanged(UserId UserId, bool ByAdministrator) : IDomainEvent;

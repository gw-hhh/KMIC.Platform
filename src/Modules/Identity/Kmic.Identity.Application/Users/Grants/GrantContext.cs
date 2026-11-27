using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Grants;

internal sealed record GrantContext(User User, bool RoleIsActive);

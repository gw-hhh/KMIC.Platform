using Kmic.BuildingBlocks.Application.Access;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Infrastructure.Access;

internal static class RoleScopeLevelExtensions
{
    // 要求 RoleScopeLevel.Code 与框架 ScopeLevel 成员名严格一致
    public static int ToScopeLevel(this RoleScopeLevel level)
    {
        ArgumentNullException.ThrowIfNull(level);

        return (int)Enum.Parse<ScopeLevel>(level.Code);
    }
}

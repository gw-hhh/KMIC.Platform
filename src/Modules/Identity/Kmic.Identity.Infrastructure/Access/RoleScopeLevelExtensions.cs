using Kmic.BuildingBlocks.Application.Access;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 角色的数据范围等级与框架访问上下文用的 <see cref="ScopeLevel" /> 之间的转换
/// </summary>
internal static class RoleScopeLevelExtensions
{
    /// <summary>
    /// 按同名编码转换为框架范围等级，并以数值存入角色快照
    /// </summary>
    public static int ToScopeLevel(this RoleScopeLevel level)
    {
        ArgumentNullException.ThrowIfNull(level);

        return (int)Enum.Parse<ScopeLevel>(level.Code);
    }
}

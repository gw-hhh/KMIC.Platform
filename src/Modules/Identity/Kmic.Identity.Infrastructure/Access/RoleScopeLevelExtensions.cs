using Kmic.BuildingBlocks.Application.Access;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// 角色的数据范围等级与框架访问上下文用的 <see cref="ScopeLevel" /> 之间的转换
/// </summary>
internal static class RoleScopeLevelExtensions
{
    /// <summary>
    /// 转成框架的等级，用数值存进角色快照；两边的编码都是成员名，有对应测试保证每个等级都转得过去
    /// </summary>
    public static int ToScopeLevel(this RoleScopeLevel level)
    {
        ArgumentNullException.ThrowIfNull(level);

        return (int)Enum.Parse<ScopeLevel>(level.Code);
    }
}

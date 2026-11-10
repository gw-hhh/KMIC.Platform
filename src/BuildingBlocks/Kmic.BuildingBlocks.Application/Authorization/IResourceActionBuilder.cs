using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 声明资源动作：把一个前端可见的动作与第 1、3、4 层检查绑定
/// </summary>
public interface IResourceActionBuilder<TResource, in TAction>
{
    /// <summary>
    /// 声明一个动作
    /// </summary>
    /// <param name="action">动作，编码即智能枚举的 Code</param>
    /// <param name="permission">权限点，必须与对应命令的 [RequirePermission] 相同（引用同一个常量）</param>
    /// <param name="state">状态检查（第 4 层），如状态机的 CanFire；没有状态限制时为空</param>
    /// <param name="readOnly">不修改数据的动作；只读模拟会话中只保留这类动作</param>
    void Action(TAction action, string permission, Func<TResource, Result>? state = null, bool readOnly = false);
}

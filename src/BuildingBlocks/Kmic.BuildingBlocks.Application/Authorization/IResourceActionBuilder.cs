using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 资源动作声明
/// </summary>
public interface IResourceActionBuilder<TResource, in TAction>
{
    /// <summary>
    /// 声明动作
    /// </summary>
    /// <param name="action">动作</param>
    /// <param name="permission">权限点，和对应命令引用同一个常量</param>
    /// <param name="state">状态检查，如状态机的 CanFire；没有状态限制时为空</param>
    /// <param name="readOnly">是否只读，只读模拟登录时只保留这类动作</param>
    void Action(TAction action, string permission, Func<TResource, Result>? state = null, bool readOnly = false);
}

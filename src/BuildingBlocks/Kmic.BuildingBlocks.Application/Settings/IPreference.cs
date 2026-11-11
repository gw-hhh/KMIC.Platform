using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 偏好
/// </summary>
/// <remarks>
/// 用不可变 record 实现，同时标 [Preference]。偏好只影响使用体验，不能用来控制权限
/// </remarks>
/// <typeparam name="TSelf">值类型自身</typeparam>
public interface IPreference<TSelf>
    where TSelf : class, IPreference<TSelf>
{
    /// <summary>
    /// 默认值
    /// </summary>
    static abstract TSelf Default { get; }

    /// <summary>
    /// 校验
    /// </summary>
    static abstract Result Validate(TSelf value);
}

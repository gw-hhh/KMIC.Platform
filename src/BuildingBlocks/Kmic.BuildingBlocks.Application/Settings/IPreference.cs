using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 偏好的值类型：不可变 record，同时标 [Preference]；偏好只影响体验，不授予任何权限
/// </summary>
/// <typeparam name="TSelf">值类型自身</typeparam>
public interface IPreference<TSelf>
    where TSelf : class, IPreference<TSelf>
{
    /// <summary>
    /// 各层都没有值时使用
    /// </summary>
    static abstract TSelf Default { get; }

    /// <summary>
    /// 写入时的校验
    /// </summary>
    static abstract Result Validate(TSelf value);
}

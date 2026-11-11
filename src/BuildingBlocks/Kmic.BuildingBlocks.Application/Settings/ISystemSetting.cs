using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 系统设置
/// </summary>
/// <remarks>
/// 用不可变 record 实现，同时标 [SystemSetting]
/// </remarks>
/// <typeparam name="TSelf">值类型自身</typeparam>
public interface ISystemSetting<TSelf>
    where TSelf : class, ISystemSetting<TSelf>
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

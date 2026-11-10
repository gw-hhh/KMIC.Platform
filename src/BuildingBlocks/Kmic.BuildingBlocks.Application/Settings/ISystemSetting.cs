using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 系统设置的值类型：不可变 record，同时标 [SystemSetting]
/// </summary>
/// <typeparam name="TSelf">值类型自身</typeparam>
public interface ISystemSetting<TSelf>
    where TSelf : class, ISystemSetting<TSelf>
{
    /// <summary>
    /// 数据库中没有值时使用，新环境开箱可用
    /// </summary>
    static abstract TSelf Default { get; }

    /// <summary>
    /// 写入时的校验，失败返回模块错误目录中的错误
    /// </summary>
    static abstract Result Validate(TSelf value);
}

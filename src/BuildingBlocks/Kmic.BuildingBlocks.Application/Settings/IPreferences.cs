namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 读取当前用户在当前角色下的偏好；由 Configuration 模块实现
/// </summary>
/// <remarks>
/// 授权、数据范围、资源策略相关的代码不得依赖它（架构测试）
/// </remarks>
public interface IPreferences
{
    /// <summary>
    /// 按层解析后的值；值类型即键
    /// </summary>
    ValueTask<T> GetAsync<T>(CancellationToken cancellationToken)
        where T : class, IPreference<T>;
}

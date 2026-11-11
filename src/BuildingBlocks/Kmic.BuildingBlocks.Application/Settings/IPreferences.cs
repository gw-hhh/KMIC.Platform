namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 当前用户的偏好
/// </summary>
/// <remarks>
/// 授权和数据权限相关的代码不能依赖它
/// </remarks>
public interface IPreferences
{
    /// <summary>
    /// 读取偏好
    /// </summary>
    ValueTask<T> GetAsync<T>(CancellationToken ct)
        where T : class, IPreference<T>;
}

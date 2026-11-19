namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 当前用户的偏好
/// </summary>
public interface IPreferences
{
    /// <summary>
    /// 读取偏好
    /// </summary>
    ValueTask<T> GetAsync<T>(CancellationToken ct)
        where T : class, IPreference<T>;
}

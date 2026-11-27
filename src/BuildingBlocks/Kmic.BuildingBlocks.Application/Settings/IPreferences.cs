namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>当前用户的偏好读取器。</summary>
public interface IPreferences
{
    ValueTask<T> GetAsync<T>(CancellationToken ct)
        where T : class, IPreference<T>;
}

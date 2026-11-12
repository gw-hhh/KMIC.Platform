using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Settings;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>
/// <see cref="IPreferences" /> 的实现：当前用户在当前角色下，本人 → 角色 → 系统 → 默认值，取第一个读得出来的
/// </summary>
public sealed class PreferencesReader(IExecutionContext execution, IPreferenceLayers layers, PreferenceCatalog catalog) : IPreferences
{
    /// <inheritdoc />
    public async ValueTask<T> GetAsync<T>(CancellationToken ct)
        where T : class, IPreference<T>
    {
        var definition = catalog.Get<T>();
        var stack = await layers.GetAsync(execution.User.UserId, execution.User.RoleId, ct);
        foreach (var (_, json) in stack.Candidates(definition.Key, definition.Layers))
        {
            if (DeclaredValues.TryRead(definition, json) is T value)
            {
                return value;
            }
        }

        return (T)definition.DefaultValue;
    }
}

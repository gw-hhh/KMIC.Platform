using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Settings;

namespace Kmic.Configuration.Application.Preferences;

/// <summary>按个人层、角色层、系统层依次选取有效偏好值，均无可用值时回退代码默认值</summary>
public sealed class PreferencesReader(IExecutionContext execution, IPreferenceLayers layers, PreferenceCatalog catalog) : IPreferences
{
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

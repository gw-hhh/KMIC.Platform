using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Configuration.Application.Preferences.Mine;

public sealed class GetMyPreferencesQueryHandler(IExecutionContext execution, IPreferenceLayers layers, PreferenceCatalog catalog)
    : IQueryHandler<GetMyPreferencesQuery, MyPreferencesResponse>
{
    public async ValueTask<Result<MyPreferencesResponse>> Handle(GetMyPreferencesQuery query, CancellationToken cancellationToken)
    {
        var access = execution.Access ?? throw new InvalidOperationException("查询个人偏好需要已登录的用户身份。");
        var stack = await layers.GetAsync(access.UserId, access.RoleId, cancellationToken);

        var items = new List<MyPreferenceItem>();
        foreach (var definition in catalog.All)
        {
            var subject = PreferenceSubject.Find(catalog, definition.Key).Value;
            var (source, json) = stack.Candidates(definition.Key, definition.Layers)
                .FirstOrDefault(c => DeclaredValues.TryRead(definition, c.Json) is not null);
            items.Add(json is null
                ? new MyPreferenceItem(definition.Key, definition.DisplayName, false, DeclaredValues.DefaultOf(definition), PreferenceSource.Default, CanSet(subject))
                : new MyPreferenceItem(definition.Key, definition.DisplayName, false, DeclaredValues.ElementOf(json), source, CanSet(subject)));
        }

        // 偏好族只返回有值的键；既无声明也不属于偏好族的历史键忽略
        foreach (var key in stack.Keys.Where(k => catalog.Find(k) is null && PreferenceFamilies.Find(k) is not null))
        {
            var subject = PreferenceSubject.Find(catalog, key).Value;
            var (source, json) = stack.Candidates(key, PreferenceLayers.All).FirstOrDefault(c => IsObject(c.Json));
            if (json is not null)
            {
                items.Add(new MyPreferenceItem(key, subject.DisplayName, true, DeclaredValues.ElementOf(json), source, true));
            }
        }

        return new MyPreferencesResponse([.. items.OrderBy(i => i.Key, StringComparer.Ordinal)]);
    }

    private static bool CanSet(PreferenceSubject subject) => subject.Layers.HasFlag(PreferenceLayers.UserRole);

    private static bool IsObject(string json)
    {
        try
        {
            return DeclaredValues.ElementOf(json).ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

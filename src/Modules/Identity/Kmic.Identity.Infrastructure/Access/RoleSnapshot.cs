using System.Collections.Frozen;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Kmic.Identity.Infrastructure.Access;

// 按角色并发版本缓存的不可变授权快照
[ImmutableObject(true)]
internal sealed record RoleSnapshot(long Version, IReadOnlyList<string> PermissionCodes, int DefaultScopeLevel, IReadOnlyDictionary<string, int> ScopeOverrides)
{
    private readonly Lazy<FrozenSet<string>> _permissions = new(() => PermissionCodes.ToFrozenSet(StringComparer.Ordinal));

    [JsonIgnore]
    public IReadOnlySet<string> Permissions => _permissions.Value;
}

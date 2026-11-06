using Spike02.Domain;
using Spike02.Infrastructure;

namespace Spike02.Tests;

internal sealed class FakeDataScope(bool unrestricted, UserId? currentUser, EffectiveScope scope) : IDataScope
{
    public int ForCalls { get; private set; }

    public bool Unrestricted { get; } = unrestricted;

    public UserId? CurrentUser { get; } = currentUser;

    public EffectiveScope For(string scopeObject)
    {
        ForCalls++;
        return scope;
    }

    public static FakeDataScope User(UserId me, params OrgUnitId[] orgUnits) => new(false, me, new EffectiveScope(false, me, orgUnits));

    public static FakeDataScope System() => new(true, null, new EffectiveScope(true, null, []));
}

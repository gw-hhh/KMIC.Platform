using Spike02.Domain;

namespace Spike02.Tests;

internal static class Ids
{
    public static readonly UserId Me = UserId.From(new Guid("019a5c3e-0000-7000-8000-00000000a001"));
    public static readonly UserId Other = UserId.From(new Guid("019a5c3e-0000-7000-8000-00000000a002"));
    public static readonly OrgUnitId East = OrgUnitId.From(new Guid("019a5c3e-0000-7000-8000-00000000b001"));
    public static readonly OrgUnitId West = OrgUnitId.From(new Guid("019a5c3e-0000-7000-8000-00000000b002"));
    public static readonly OrgUnitId North = OrgUnitId.From(new Guid("019a5c3e-0000-7000-8000-00000000b003"));
    public static readonly SampleId Cursor = SampleId.From(new Guid("019a5c3e-0000-7000-8000-00000000c001"));
}

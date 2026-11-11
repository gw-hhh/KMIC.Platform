using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

internal sealed class ThingStatus : Enumeration<ThingStatus>
{
    public static readonly ThingStatus Draft = new("Draft", "草稿");
    public static readonly ThingStatus Done = new("Done", "已完成");

    private ThingStatus(string code, string name)
        : base(code, name)
    {
    }
}

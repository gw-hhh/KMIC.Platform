using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Application.Tests.Serialization;

internal sealed class Shade : Enumeration<Shade>
{
    public static readonly Shade Red = new("Red", "红");
    public static readonly Shade Blue = new("Blue", "蓝");

    private Shade(string code, string name)
        : base(code, name)
    {
    }
}

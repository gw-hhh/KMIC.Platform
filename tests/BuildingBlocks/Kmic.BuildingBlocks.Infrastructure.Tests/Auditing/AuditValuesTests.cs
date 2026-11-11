using System.Globalization;
using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Auditing;

public sealed class AuditValuesTests
{
    [Theory]
    [InlineData(AuditMask.Phone, "13812341234", "138****1234")]
    [InlineData(AuditMask.IdNumber, "110101199001011234", "110***********1234")]
    [InlineData(AuditMask.Email, "zhang@example.com", "z***@example.com")]
    [InlineData(AuditMask.Phone, "12345", "*****")]
    [InlineData(AuditMask.Email, "no-at-sign", "**********")]
    public void Mask_KeepsHeadAndTailOnly(AuditMask mask, string value, string expected) =>
        AuditValues.Mask(value, mask).ShouldBe(expected);

    [Fact]
    public void Format_UsesInvariantRepresentations()
    {
        var instant = new DateTimeOffset(2026, 11, 12, 8, 30, 0, TimeSpan.FromHours(8));
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
        try
        {
            AuditValues.Format(1234.5m).ShouldBe("1234.5");
            AuditValues.Format(instant).ShouldBe("2026-11-12T08:30:00.0000000+08:00");
            AuditValues.Format(true).ShouldBe("true");
            AuditValues.Format(null).ShouldBeNull();
            AuditValues.Format("原样").ShouldBe("原样");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Truncate_LongValue_KeepsHeadAndHashOfWhole()
    {
        var value = new string('甲', 30);

        var truncated = AuditValues.Truncate(value, 10).ShouldNotBeNull();

        truncated.ShouldStartWith(new string('甲', 10) + "…(sha256:");
        string.Equals(truncated, AuditValues.Truncate(new string('甲', 31), 10), StringComparison.Ordinal).ShouldBeFalse();
        AuditValues.Truncate("短", 10).ShouldBe("短");
    }
}

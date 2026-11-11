using System.Text.Json;
using Kmic.BuildingBlocks.Application.Serialization;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Serialization;

public sealed class KmicJsonOptionsTests
{
    private const string Valid = """{"name":"墙漆","shade":"Red","finish":"Matte","mixedAt":"2026-11-04T16:30:00+08:00","coats":2}""";

    // 这几项不是开关，单独比较
    private static readonly string[] _notSettings =
        [nameof(JsonSerializerOptions.Converters), nameof(JsonSerializerOptions.TypeInfoResolver), nameof(JsonSerializerOptions.TypeInfoResolverChain), nameof(JsonSerializerOptions.IsReadOnly)];

    [Fact]
    public void Configure_WebDefaults_MatchCreateOnEverySetting()
    {
        var expected = KmicJsonOptions.Create();
        var target = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        KmicJsonOptions.Configure(target);

        foreach (var property in typeof(JsonSerializerOptions).GetProperties().Where(p => p.GetIndexParameters().Length == 0 && !_notSettings.Contains(p.Name)))
        {
            property.GetValue(target).ShouldBe(property.GetValue(expected), property.Name);
        }

        target.Converters.Select(c => c.GetType()).ShouldBe(expected.Converters.Select(c => c.GetType()));
    }

    [Fact]
    public void Serialize_WritesCodesMemberNamesAndUtc()
    {
        var paint = new Paint("Primer", Shade.Blue, PaintFinish.Gloss, new DateTimeOffset(2026, 11, 4, 16, 30, 0, TimeSpan.FromHours(8)), 2);

        var json = JsonSerializer.Serialize(paint, KmicJsonOptions.Default);

        json.ShouldBe("""{"name":"Primer","shade":"Blue","finish":"Gloss","mixedAt":"2026-11-04T08:30:00Z","coats":2}""");
    }

    [Fact]
    public void Deserialize_OffsetTime_ConvertedToUtc()
    {
        var paint = JsonSerializer.Deserialize<Paint>(Valid, KmicJsonOptions.Default);

        paint.ShouldNotBeNull();
        paint.Shade.ShouldBe(Shade.Red);
        paint.MixedAt.ShouldBe(new DateTimeOffset(2026, 11, 4, 8, 30, 0, TimeSpan.Zero));
        paint.MixedAt.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Theory]
    [InlineData("""{"name":"墙漆","shade":"Green","finish":"Matte","mixedAt":"2026-11-04T08:30:00Z","coats":2}""", "$.shade")]
    [InlineData("""{"name":"墙漆","shade":"Red","finish":"Matte","mixedAt":"2026-11-04T08:30:00","coats":2}""", "$.mixedAt")]
    [InlineData("""{"name":"墙漆","shade":"Red","finish":1,"mixedAt":"2026-11-04T08:30:00Z","coats":2}""", "$.finish")]
    [InlineData("""{"name":"墙漆","shade":"Red","finish":"Matte","mixedAt":"2026-11-04T08:30:00Z","coats":"2"}""", "$.coats")]
    public void Deserialize_InvalidValue_RejectedWithPath(string json, string path)
    {
        var thrown = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Paint>(json, KmicJsonOptions.Default));

        thrown.Path.ShouldBe(path);
    }

    [Theory]
    [InlineData("""{"name":"墙漆","shade":"Red","finish":"Matte","mixedAt":"2026-11-04T08:30:00Z","coats":2,"brand":"x"}""")]
    [InlineData("""{"Name":"墙漆","shade":"Red","finish":"Matte","mixedAt":"2026-11-04T08:30:00Z","coats":2}""")]
    [InlineData("""{"name":"墙漆","name":"底漆","shade":"Red","finish":"Matte","mixedAt":"2026-11-04T08:30:00Z","coats":2}""")]
    [InlineData("""{"name":null,"shade":"Red","finish":"Matte","mixedAt":"2026-11-04T08:30:00Z","coats":2}""")]
    public void Deserialize_UnknownCaseDuplicateOrNull_Rejected(string json) =>
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Paint>(json, KmicJsonOptions.Default));

    [Fact]
    public void Stored_UnknownField_Ignored()
    {
        var json = Valid.Replace("}", ""","removedInNewVersion":true}""", StringComparison.Ordinal);

        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Paint>(json, KmicJsonOptions.Default));
        JsonSerializer.Deserialize<Paint>(json, KmicJsonOptions.Stored).ShouldNotBeNull().Name.ShouldBe("墙漆");
    }

    [Fact]
    public void SharedInstances_ReadOnly()
    {
        KmicJsonOptions.Default.IsReadOnly.ShouldBeTrue();
        KmicJsonOptions.Stored.IsReadOnly.ShouldBeTrue();
    }
}

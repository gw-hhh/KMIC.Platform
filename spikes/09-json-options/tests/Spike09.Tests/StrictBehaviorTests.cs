using System.Text.Json;
using System.Text.Json.Serialization;
using Shouldly;
using Xunit;

namespace Spike09.Tests;

// 在 Strict 预设上加小驼峰、字符串枚举与深度限制，逐条核对 05-06 第 5.1 节要求的行为
public sealed class StrictBehaviorTests
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerOptions.Strict)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        MaxDepth = 32,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    private const string ValidCreate = """{"title":"示例","quantity":1,"kind":"Urgent","note":null}""";

    public enum SampleKind
    {
        Standard,
        Urgent,
    }

    [Fact]
    public void Deserialize_ValidPayload_Succeeds()
    {
        var request = JsonSerializer.Deserialize<CreateRequest>(ValidCreate, _options);

        request.ShouldBe(new CreateRequest("示例", 1, SampleKind.Urgent, null));
    }

    [Fact]
    public void Deserialize_DuplicateProperty_Throws() =>
        Fails<CreateRequest>("""{"title":"a","title":"b","quantity":1,"kind":"Urgent","note":null}""");

    [Fact]
    public void Deserialize_UnknownProperty_Throws() =>
        Fails<CreateRequest>("""{"title":"a","quantity":1,"kind":"Urgent","note":null,"extra":1}""");

    [Fact]
    public void Deserialize_NullForNonNullableConstructorParameter_Throws() =>
        Fails<CreateRequest>("""{"title":null,"quantity":1,"kind":"Urgent","note":null}""");

    [Fact]
    public void Deserialize_NullForNonNullableProperty_Throws() =>
        Fails<UpdateRequest>("""{"title":"a","remark":null}""");

    [Fact]
    public void Deserialize_MissingConstructorParameter_Throws() =>
        Fails<CreateRequest>("""{"quantity":1,"kind":"Urgent","note":null}""");

    [Fact]
    public void Deserialize_MissingRequiredProperty_Throws() =>
        Fails<UpdateRequest>("""{"remark":"x"}""");

    [Fact]
    public void Deserialize_MissingNonRequiredProperty_KeepsDefault()
    {
        // 不可空但既不是 required 也不是构造参数的属性，缺失时不报错，保留初始值
        var request = JsonSerializer.Deserialize<UpdateRequest>("""{"title":"a"}""", _options);

        request.ShouldNotBeNull().Remark.ShouldBe(string.Empty);
    }

    [Fact]
    public void Deserialize_NumberAsString_Throws() =>
        Fails<CreateRequest>("""{"title":"a","quantity":"1","kind":"Urgent","note":null}""");

    [Fact]
    public void Deserialize_EnumAsInteger_Throws() =>
        Fails<CreateRequest>("""{"title":"a","quantity":1,"kind":1,"note":null}""");

    [Fact]
    public void Deserialize_EnumNameInOtherCase_Accepted()
    {
        var request = JsonSerializer.Deserialize<CreateRequest>("""{"title":"a","quantity":1,"kind":"urgent","note":null}""", _options);

        request.ShouldNotBeNull().Kind.ShouldBe(SampleKind.Urgent);
    }

    [Fact]
    public void Deserialize_PropertyNameInOtherCase_Throws() =>
        Fails<CreateRequest>("""{"Title":"a","quantity":1,"kind":"Urgent","note":null}""");

    [Fact]
    public void Deserialize_DeeperThanMaxDepth_Throws()
    {
        var json = string.Concat(Enumerable.Repeat("""{"child":""", 40)) + "null" + new string('}', 40);

        Fails<Node>(json);
    }

    [Fact]
    public void Deserialize_DateTimeOffsetWithoutOffset_AcceptedAsLocalTime()
    {
        // 内置行为会把不带偏移的时刻当作服务器本地时间，所以 05-06 要求框架转换器拒绝这种输入
        var moment = JsonSerializer.Deserialize<Moment>("""{"at":"2026-11-04T08:30:00"}""", _options);

        var expectedOffset = TimeZoneInfo.Local.GetUtcOffset(new DateTime(2026, 11, 4, 8, 30, 0, DateTimeKind.Unspecified));
        moment.ShouldNotBeNull().At.Offset.ShouldBe(expectedOffset);
    }

    [Fact]
    public void Serialize_NullProperty_WrittenAsNull()
    {
        var json = JsonSerializer.Serialize(new CreateRequest("a", 1, SampleKind.Standard, null), _options);

        json.ShouldBe("""{"title":"a","quantity":1,"kind":"Standard","note":null}""");
    }

    [Fact]
    public void Deserialize_NumberAsString_AcceptedByAspNetCoreDefaults()
    {
        // ASP.NET Core 的默认选项基于 Web 预设，数字可以用字符串传，属性名不区分大小写
        var web = new Microsoft.AspNetCore.Http.Json.JsonOptions().SerializerOptions;

        var request = JsonSerializer.Deserialize<CreateRequest>("""{"Title":"a","quantity":"1","kind":0,"note":null}""", web);

        request.ShouldBe(new CreateRequest("a", 1, SampleKind.Standard, null));
    }

    private static void Fails<T>(string json)
    {
        var exception = Should.Throw<JsonException>(() => JsonSerializer.Deserialize<T>(json, _options));
        TestContext.Current.TestOutputHelper?.WriteLine($"Path = {exception.Path ?? "null"}\n{exception.Message}");
    }

    public sealed record CreateRequest(string Title, int Quantity, SampleKind Kind, string? Note);

    public sealed class UpdateRequest
    {
        public required string Title { get; init; }

        public string Remark { get; init; } = string.Empty;
    }

    public sealed class Node
    {
        public Node? Child { get; init; }
    }

    public sealed record Moment(DateTimeOffset At);
}

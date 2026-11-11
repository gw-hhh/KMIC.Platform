using System.Net;
using System.Text;
using System.Text.Json;
using Kmic.BuildingBlocks.Presentation.Tests.Cases;
using Kmic.BuildingBlocks.Presentation.Tests.Support;
using Shouldly;
using static Kmic.BuildingBlocks.Presentation.Tests.Support.Messages;

namespace Kmic.BuildingBlocks.Presentation.Tests.Serialization;

/// <summary>
/// 请求与响应的 JSON 约定：回显端点把请求体原样交给命令再原样返回
/// </summary>
public sealed class RequestJsonTests : IAsyncLifetime
{
    private const string Id = "0192f3a0-7c1e-7d3a-9b3e-5a4f2c1d8e90";

    private ApiHost _api = null!;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _api = await ApiHost.StartAsync([ThingEndpoints.Module()]);
        _api.Script.On<EchoCommand, EchoPayload>((command, _) => command.Payload);
    }

    public async ValueTask DisposeAsync() => await _api.DisposeAsync();

    [Fact]
    public async Task ValidRequest_RoundTrips_InTheAgreedFormats()
    {
        var response = await PostAsync(Body());

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var text = await response.Content.ReadAsStringAsync(Ct);
        var body = JsonDocument.Parse(text).RootElement;

        body.GetProperty("id").GetString().ShouldBe(Id);                                       // 强类型 ID 是 UUID 字符串
        body.GetProperty("status").GetString().ShouldBe("Done");                               // 智能枚举是 Code
        body.GetProperty("at").GetString().ShouldBe("2026-11-04T08:30:00Z");                   // 带偏移的时刻转成 UTC
        body.GetProperty("day").GetString().ShouldBe("2026-11-04");
        body.GetProperty("time").GetString().ShouldBe("08:30:00");
        body.GetProperty("amount").GetDecimal().ShouldBe(12.50m);                              // 金额是数字
        body.GetProperty("remark").ValueKind.ShouldBe(JsonValueKind.Null);                     // 空值输出 null，不省略字段
        body.EnumerateObject().Select(p => p.Name).ShouldBe(["id", "status", "at", "day", "time", "amount", "name", "remark", "count"]);
        text.ShouldContain("华东");                                                             // 中文原样输出
    }

    [Fact]
    public async Task HtmlSensitiveCharacters_AreStillEscaped()
    {
        var response = await PostAsync(Body(name: "<b>&</b>"));

        var text = await response.Content.ReadAsStringAsync(Ct);

        text.ShouldNotContain("<b>");
        text.ShouldContain("\\u003Cb\\u003E");
    }

    [Theory]
    [InlineData("""{"id":"0192f3a0-7c1e-7d3a-9b3e-5a4f2c1d8e90","status":"Done","at":"2026-11-04T16:30:00+08:00","day":"2026-11-04","time":"08:30:00","amount":12.5,"name":"a","remark":null,"count":3,"extra":1}""", "未知字段")]
    [InlineData("""{"id":"0192f3a0-7c1e-7d3a-9b3e-5a4f2c1d8e90","status":"Done","at":"2026-11-04T16:30:00+08:00","day":"2026-11-04","time":"08:30:00","amount":12.5,"Name":"a","remark":null,"count":3}""", "属性名大小写不对")]
    [InlineData("""{"id":"0192f3a0-7c1e-7d3a-9b3e-5a4f2c1d8e90","status":"Done","at":"2026-11-04T16:30:00+08:00","day":"2026-11-04","time":"08:30:00","amount":12.5,"name":"a","name":"b","remark":null,"count":3}""", "重复属性")]
    [InlineData("""{"id":"0192f3a0-7c1e-7d3a-9b3e-5a4f2c1d8e90","status":"Done","at":"2026-11-04T16:30:00+08:00","day":"2026-11-04","time":"08:30:00","amount":12.5,"name":null,"remark":null,"count":3}""", "不可空字段收到 null")]
    [InlineData("""{"id":"0192f3a0-7c1e-7d3a-9b3e-5a4f2c1d8e90","status":"Done","at":"2026-11-04T16:30:00+08:00","day":"2026-11-04","time":"08:30:00","amount":12.5,"remark":null,"count":3}""", "缺少必填字段")]
    [InlineData("""{"id":"0192f3a0-7c1e-7d3a-9b3e-5a4f2c1d8e90","status":"Done","at":"2026-11-04T16:30:00+08:00","day":"2026-11-04","time":"08:30:00","amount":12.5,"name":"a","remark":null,"count":"3"}""", "数字写成字符串")]
    [InlineData("""{"id":"0192f3a0-7c1e-7d3a-9b3e-5a4f2c1d8e90","status":"Done","at":"2026-11-04T16:30:00+08:00","day":"2026-11-04","time":"08:30:00","amount":"12.5","name":"a","remark":null,"count":3}""", "金额写成字符串")]
    [InlineData("""{"id":"0192f3a0-7c1e-7d3a-9b3e-5a4f2c1d8e90","status":"Done","at":"2026-11-04T16:30:00+08:00","day":"2026-11-04","time":"08:30:00","amount":12.5,"name":"a","remark":null,"count":3,""", "JSON 不完整")]
    [InlineData("not json", "不是 JSON")]
    public async Task InvalidBody_Returns400_MalformedRequest(string json, string reason)
    {
        var response = await PostAsync(json);

        var body = await response.ShouldBeProblemAsync(400, "Common.MalformedRequest");
        body.GetProperty("errors").GetArrayLength().ShouldBe(1, reason);
        body.GetProperty("errors")[0].GetProperty("code").GetString().ShouldBe("Malformed", reason);
    }

    [Theory]
    [InlineData("status", "\"Bogus\"")]
    [InlineData("status", "1")]
    [InlineData("at", "\"2026-11-04T08:30:00\"")]
    [InlineData("at", "\"not a time\"")]
    [InlineData("id", "\"not-a-uuid\"")]
    [InlineData("day", "\"2026/11/04\"")]
    [InlineData("count", "\"3\"")]
    [InlineData("count", "1.5")]
    public async Task InvalidValue_Returns400_WithTheFieldPath(string field, string value)
    {
        var response = await PostAsync(Body(replace: (field, value)));

        var body = await response.ShouldBeProblemAsync(400, "Common.MalformedRequest");
        body.GetProperty("errors")[0].GetProperty("path").GetString().ShouldBe(field);
    }

    [Fact]
    public async Task TimeWithoutOffset_IsRejected_NotTakenAsServerLocalTime()
    {
        var response = await PostAsync(Body(replace: ("at", "\"2026-11-04T08:30:00\"")));

        await response.ShouldBeProblemAsync(400, "Common.MalformedRequest");
    }

    [Fact]
    public async Task EmptyBody_Returns400()
    {
        var response = await PostAsync(string.Empty);

        await response.ShouldBeProblemAsync(400, "Common.MalformedRequest");
    }

    private static string Body(string name = "华东", (string Field, string Value)? replace = null)
    {
        var fields = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["id"] = $"\"{Id}\"",
            ["status"] = "\"Done\"",
            ["at"] = "\"2026-11-04T16:30:00+08:00\"",
            ["day"] = "\"2026-11-04\"",
            ["time"] = "\"08:30:00\"",
            ["amount"] = "12.50",
            ["name"] = JsonSerializer.Serialize(name),
            ["remark"] = "null",
            ["count"] = "3",
        };
        if (replace is { } r)
        {
            fields[r.Field] = r.Value;
        }

        return "{" + string.Join(",", fields.Select(f => $"\"{f.Key}\":{f.Value}")) + "}";
    }

    private async Task<HttpResponseMessage> PostAsync(string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _api.Client.PostAsync("/api/v1/samples/echo", content, Ct);
    }
}

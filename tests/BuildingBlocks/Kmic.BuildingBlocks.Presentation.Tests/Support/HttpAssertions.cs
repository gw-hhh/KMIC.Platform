using System.Text.Json;
using Shouldly;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

internal static class HttpAssertions
{
    private static readonly string[] _problemMembers = ["type", "title", "status", "detail", "instance", "code", "traceId", "retryable", "errors", "currentVersion"];

    public static async Task<JsonElement> JsonAsync(this HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    /// <summary>
    /// 断言是统一格式的错误响应，返回解析后的响应体
    /// </summary>
    public static async Task<JsonElement> ShouldBeProblemAsync(this HttpResponseMessage response, int status, string code)
    {
        ((int)response.StatusCode).ShouldBe(status);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.JsonAsync();
        body.GetProperty("code").GetString().ShouldBe(code);
        body.GetProperty("type").GetString().ShouldBe($"urn:kmic:error:{code}");
        body.GetProperty("status").GetInt32().ShouldBe(status);
        body.GetProperty("title").GetString().ShouldNotBeNullOrWhiteSpace();
        body.GetProperty("retryable").ValueKind.ShouldBeOneOf(JsonValueKind.True, JsonValueKind.False);

        // 响应头和响应体里的追踪 ID 是同一个，用户报障时给哪个都行
        response.Headers.GetValues("X-Trace-Id").ShouldHaveSingleItem().ShouldBe(body.GetProperty("traceId").GetString());

        // 只有白名单里的字段
        body.EnumerateObject().Select(p => p.Name).ShouldAllBe(name => _problemMembers.Contains(name));
        return body;
    }
}

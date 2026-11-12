using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kmic.BuildingBlocks.Testing.Database;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Kmic.Api.Tests.Identity;

/// <summary>
/// Identity 认证流程的测试基础：真实的访问上下文加载器、真实签发的令牌，数据直接造在 Identity 的表里
/// </summary>
public abstract class IdentityApiTestBase(ClassDatabase<ApiTemplate> database, ApiTemplate template, IReadOnlyDictionary<string, string>? settings = null) : IAsyncLifetime
{
    protected const string Origin = "https://app.test";

    private readonly TokenFactory _tokens = new();

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    internal IdentitySeed Seed { get; private set; } = null!;

    protected ApiFactory Factory { get; private set; } = null!;

    protected string ConnectionString => template.RuntimeConnectionString(database.Name);

    public ValueTask InitializeAsync()
    {
        var merged = new Dictionary<string, string>(StringComparer.Ordinal) { ["Identity:Cookie:TrustedOrigins:0"] = Origin };
        foreach (var (key, value) in settings ?? new Dictionary<string, string>(StringComparer.Ordinal))
        {
            merged[key] = value;
        }

        Factory = new ApiFactory(ConnectionString, _tokens, realIdentity: true, merged);
        Seed = new IdentitySeed(ConnectionString);
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await Factory.DisposeAsync();
        _tokens.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 不自动处理 Cookie 的客户端：刷新令牌 Cookie 带 Secure，而测试走的是 http，自己读 Set-Cookie 再手工带上更直接
    /// </summary>
    protected HttpClient NewClient() => Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

    protected static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync(Ct);
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    protected static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, object? body, string? bearer = null, bool csrf = false, string? cookie = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = body is null ? null : JsonContent.Create(body) };
        if (bearer is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        }

        if (csrf)
        {
            request.Headers.Add("Origin", Origin);
            request.Headers.Add("X-Requested-With", "kmic");
        }

        if (cookie is not null)
        {
            request.Headers.Add("Cookie", cookie);
        }

        return await client.SendAsync(request, Ct);
    }

    protected static async Task<HttpResponseMessage> GetAsync(HttpClient client, string url, string? bearer)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (bearer is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        }

        return await client.SendAsync(request, Ct);
    }

    /// <summary>
    /// 从响应的 Set-Cookie 里取刷新令牌 Cookie 的“名=值”，没有时为空
    /// </summary>
    protected static string? RefreshCookieOf(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.Select(v => v.Split(';')[0]).FirstOrDefault(v => v.StartsWith("__Host-kmic-rt=", StringComparison.Ordinal) && v.Length > "__Host-kmic-rt=".Length)
            : null;
}
